namespace Saffiano
{
    using System;
    using System.Collections.Generic;

    public class Atlas : Texture
    {
        internal class ReferenceDescriptor
        {
            public Atlas atlas { get; private set; } = null;

            public Allocator.Line line { get; private set; } = null;

            public Allocator.Segment segment { get; private set; } = null;

            internal ReferenceDescriptor(Atlas atlas, Allocator.Line line, uint height, Allocator.Segment segment)
            {
                this.atlas = atlas;
                this.line = line;
                this.height = height;
                this.segment = segment;
            }

            public uint x
            {
                get
                {
                    uint result = 0;
                    foreach (var child in this.line.children)
                    {
                        if (segment == child)
                        {
                            return result;
                        }
                        result += child.length;
                    }
                    throw new Exception();
                }
            }

            public uint y
            {
                get
                {
                    uint result = 0;
                    foreach (var child in atlas.allocator.lines)
                    {
                        if (this.line == child)
                        {
                            return result;
                        }
                        result += child.height;
                    }
                    throw new Exception();
                }
            }

            public uint width
            {
                get => segment.length - atlas.allocator.spacing;
            }

            public uint height { get; private set; }
        }

        internal class Allocator
        {
            private static uint GetMinimumPowerOfTwoLargerThanN(uint n)
            {
                if (n == 0)
                {
                    throw new NotImplementedException("N of 0 is irrelevant");
                }
                uint result = 1;
                while (result < n)
                {
                    result <<= 1;
                }
                return result;
            }

            public class Segment
            {
                public uint start { get; set; }

                public uint length { get; set; }

                public bool allocated { get; set; }
            }

            public class Line : Segment
            {
                public List<Segment> children = new List<Segment>();

                public uint height { get => this.length; }

                public uint width { get; private set; }

                public uint spacing { get; private set; }

                public Line(uint width, uint spacing) : base()
                {
                    this.width = width;
                    this.spacing = spacing;
                    children.Add(new Segment() { start = 0, length = width, allocated = false });
                    Allocate(0);
                }

                public Segment Allocate(uint length)
                {
                    var spacingLength = length + spacing;
                    Segment greedy = null;
                    foreach (var child in children)
                    {
                        if (child.length < spacingLength || child.allocated)
                        {
                            continue;
                        }
                        if (greedy == null)
                        {
                            greedy = child;
                        }
                        if (greedy.length < child.length)
                        {
                            greedy = child;
                        }
                    }
                    if (greedy == null)
                    {
                        return null;
                    }
                    int index = children.IndexOf(greedy);
                    children.Remove(greedy);
                    var added = new Segment() { start = greedy.start, length = spacingLength, allocated = true };
                    children.Insert(index, added);
                    if (greedy.length > spacingLength)
                    {
                        children.Insert(index + 1, new Segment() { start = greedy.start + spacingLength, length = greedy.length - spacingLength, allocated = false });
                    }
                    return added;
                }

                public void Free(Segment segment)
                {
                    int index = children.IndexOf(segment);
                    Segment previous = null;
                    Segment next = null;
                    if (index > 0)
                    {
                        if (!children[index - 1].allocated)
                        {
                            previous = children[index - 1];
                        }
                    }
                    if (index < children.Count - 1)
                    {
                        if (!children[index + 1].allocated)
                        {
                            next = children[index + 1];
                        }
                    }
                    if (previous == null && next == null)
                    {
                        children[index].allocated = false;
                    }
                    else if (previous == null)
                    {
                        children[index].length += children[index + 1].length;
                        children.RemoveAt(index + 1);
                    }
                    else if (next == null)
                    {
                        children[index - 1].length += children[index].length;
                        children.RemoveAt(index);
                    }
                    else
                    {
                        children[index - 1].length += children[index].length;
                        children[index - 1].length += children[index + 1].length;
                        children.RemoveAt(index + 1);
                        children.RemoveAt(index);
                    }
                }
            }

            public uint spacing { get; private set; }

            public uint width { get; private set; }

            public uint height { get; private set; }

            public Atlas atlas { get; private set; }

            public List<Line> lines = new List<Line>();

            public Allocator(Atlas atlas, uint width, uint height, uint spacing)
            {
                this.atlas = atlas;
                this.width = width;
                this.height = height;
                this.spacing = spacing;
                lines.Add(new Line(width, spacing) { start = 0, length = height, allocated = false });
                Allocate(width - spacing * 2, 0);
            }

            public ReferenceDescriptor Allocate(uint width, uint height)
            {
                var spacingHeight = height + spacing;
                uint p2 = GetMinimumPowerOfTwoLargerThanN(spacingHeight);
                foreach (var line in lines)
                {
                    if (line.height != p2 || !line.allocated)
                    {
                        continue;
                    }
                    var tmp0 = line.Allocate(width);
                    if (tmp0 != null)
                    {
                        return new ReferenceDescriptor(this.atlas, line, height, tmp0);
                    }
                }
                Line greedy = null;
                foreach (var line in lines)
                {
                    if (line.height < p2 || line.allocated)
                    {
                        continue;
                    }
                    if (greedy == null)
                    {
                        greedy = line;
                    }
                    if (line.height < greedy.height)
                    {
                        greedy = line;
                    }
                }
                if (greedy == null)
                {
                    return null;
                }
                int index = lines.IndexOf(greedy);
                lines.Remove(greedy);
                var added = new Line(this.width, spacing) { start = greedy.start, length = p2, allocated = true };
                lines.Insert(index, added);
                if (greedy.height > spacingHeight)
                {
                    lines.Insert(index + 1, new Line(this.width, spacing) { start = greedy.start + spacingHeight, length = greedy.height - p2, allocated = false });
                }
                var tmp1 = added.Allocate(width);
                if (tmp1 == null)
                {
                    return null;
                }
                return new ReferenceDescriptor(this.atlas, added, height, tmp1);
            }

            public void Free(ReferenceDescriptor referenceDescriptor)
            {
                referenceDescriptor.line.Free(referenceDescriptor.segment);

                var children = referenceDescriptor.line.children;
                if (children.Count != 1 || children[0].allocated)
                {
                    return;
                }

                int index = lines.IndexOf(referenceDescriptor.line);
                Line previous = null;
                Line next = null;
                if (index > 0)
                {
                    if (!lines[index - 1].allocated)
                    {
                        previous = lines[index - 1];
                    }
                }
                if (index < lines.Count - 1)
                {
                    if (!lines[index + 1].allocated)
                    {
                        next = lines[index + 1];
                    }
                }
                if (previous == null && next == null)
                {
                    lines[index].allocated = false;
                }
                else if (previous == null)
                {
                    lines[index].length += lines[index + 1].length;
                    lines.RemoveAt(index + 1);
                }
                else if (next == null)
                {
                    lines[index - 1].length += lines[index].length;
                    lines.RemoveAt(index);
                }
                else
                {
                    lines[index - 1].length += lines[index].length;
                    lines[index - 1].length += lines[index + 1].length;
                    lines.RemoveAt(index + 1);
                    lines.RemoveAt(index);
                }
            }
        }

        private Allocator allocator = null;

        private HashSet<Texture> children = new HashSet<Texture>();

        public Atlas(uint width, uint height, uint spacing = 0) : base(width, height)
        {
            allocator = new Allocator(this, width, height, spacing);
        }

        ~Atlas()
        {
            allocator = null;
        }

        public Texture Allocate(uint width, uint height)
        {
            return new Texture(allocator.Allocate(width, height));
        }

        internal void Free(Texture texture)
        {
            if (texture.atlas != this)
            {
                throw new Exception("The atlas does not contain the texture");
            }
            allocator.Free(texture.referenceDescriptor);
        }
    }
}
