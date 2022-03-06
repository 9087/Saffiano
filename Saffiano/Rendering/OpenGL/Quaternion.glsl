
mat3 __Quaternion__create_mat3_from_euler_angle(vec3 euler)
{
  // https://www.geeks3d.com/20141114/glsl-4x4-matrix-mat4-fields/

  float cosX = cos(euler.x);
  float sinX = sin(euler.x);
  float cosY = cos(euler.y);
  float sinY = sin(euler.y);
  float cosZ = cos(euler.z);
  float sinZ = sin(euler.z);

  mat3 m;

  m[0][0] = cosY * cosZ + sinX * sinY * sinZ;
  m[0][1] = cosY * sinZ - sinX * sinY * cosZ;
  m[0][2] = cosX * sinY;
 
  m[1][0] = -cosX * sinZ;
  m[1][1] = cosX * cosZ;
  m[1][2] = sinX;

  m[2][0] = sinX * cosY * sinZ - sinY * cosZ;
  m[2][1] = -sinY * sinZ - sinX * cosY * cosZ;
  m[2][2] = cosX * cosY;

  return m;
}
