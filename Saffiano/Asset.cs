using System.IO;

namespace Saffiano
{
    public class Asset
    {
        protected string extension = null;
        protected string filePath = null;

        public Asset(string filePath)
        {
            this.filePath = filePath;
            extension = Path.GetExtension(filePath).ToUpper();
        }
    }
}
