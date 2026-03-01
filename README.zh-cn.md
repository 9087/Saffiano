# Saffiano

[English](https://github.com/9087/Saffiano) | **简体中文**

基于.NET Core平台参考Unity接口风格的玩具游戏引擎。

![](Documents/Snapshot.gif)

## 功能列举

### 使用C#语言开发材质

渲染层基于OpenGL实现。底层提供一套IL到GLSL语言的编译器（关键入口是ShaderCompiler.Compile函数），继承`ScriptableMaterial`类型可以直接使用C#开发材质，而无需书写GLSL代码。最终在操纵材质时，可以优雅的借助编辑器的智能提示访问和设置参数。

下列代码是Phong光照模型的C#实现：

```C#
public class Phong : Lambert
{
	[Uniform]
	public virtual float shininess { get; set; } = 32;

	[Uniform]
	public Vector3 cameraPosition => Camera.main.transform.position;

	protected Vector4 GetSpecularColor(Vector3 worldPosition, Vector3 worldNormal)
	{
		var r = Vector3.Reflect(-directionLight.normalized, worldNormal) * Mathf.Max(Vector3.Dot(worldNormal, directionLight), 0);
		var viewDirection = (-worldPosition + cameraPosition).normalized;
		var specularColor = (Vector4)directionLightColor * Mathf.Pow(Mathf.Max(Vector3.Dot(viewDirection, r), 0), shininess);
		return specularColor;
	}

	public virtual void VertexShader(
		[Attribute(AttributeType.Position)] Vector3 a_position,
		[Attribute(AttributeType.Normal)] Vector3 a_normal,
		out Vector4 gl_Position,
		out Vector4 v_position,
		out Vector3 v_normal
	)
	{
		gl_Position = mvp * new Vector4(a_position, 1.0f);
		v_position = new Vector4(a_position, 1.0f);
		v_normal = a_normal;
	}

	public virtual void FragmentShader(
		Vector4 v_position,
		Vector3 v_normal,
		out Color f_color
	)
	{
		Vector3 worldNormal = (mv * new Vector4(v_normal, 0)).xyz.normalized;
		var diffuseColor = (Color)GetDiffuseColor(((Vector4)directionLightColor).xyz, worldNormal);
		var specularColor = GetSpecularColor((mv * v_position).xyz, worldNormal);
		f_color = (Color)(specularColor + (Vector4)diffuseColor + (Vector4)(ambientColor));
	}
}
```

`Uniform`参数需要使用`Uniform`标记。重写`VertexShader`和`FragmentShader`函数实现`Phong`光照。实际使用时，直接设置`shininess`即可完成反光度的设置：

```C#
var phong = new Resources.Default.Material.Phong();
phong.shininess = 16;

GameObject bunny = new GameObject("Bunny");
bunny.AddComponent<Transform>();
bunny.AddComponent<MeshFilter>();
bunny.AddComponent<MeshRenderer>().material = phong;
bunny.AddComponent<MeshAsyncLoader>().path = "models/bunny/reconstruction/bun_zipper.ply";
```

当前支持的Shader类型有：`VertexShader`、`FragmentShader`、`GeometryShader`和`TessEvaluationShader`。其中[`Saffiano.Gallery.Assets.Objects.GrassMaterial`](/Saffiano.Gallery/Assets/Objects/Grass.cs)类型展示了`GeometryShader`和`TessEvaluationShader`类型的使用样例（即上文截图草的材质实现）。

### Coroutine

[`Saffiano.Coroutine`](Saffiano/Coroutine.cs)提供了类Unity的协程实现。当前实现了`WaitForSeconds`和`ResourceRequest`两个`YieldInstruction`。

### 资源加载

使用`Resources.LoadAsync`即可加载资源，返回的`ResourceRequest`对象可以在Coroutine中进行`yield return`。例如[`ResourceAsyncLoader`](/Saffiano.Gallery/Assets/Common/ResourceAsyncLoader.cs)实现如下：

```C#
public class ResourceAsyncLoader : Behaviour
{
	public string path = null;
	protected ResourceRequest resourceRequest;

	void Start()
	{
		this.StartCoroutine(this.Load());
	}

	public virtual IEnumerator Load()
	{
		this.resourceRequest = Resources.LoadAsync(this.path);
		yield return this.resourceRequest;
	}
}
```

当前支持PLY和PNG格式资源。

### UI

提供Image、Text、LayoutGroup（布局）、InputField（键盘输入）、Button（鼠标输入）、Shadow等一系列组件。文本借助`FreeTypeSharp`库进行渲染，`Glyph`贴图合并在字体图集中。