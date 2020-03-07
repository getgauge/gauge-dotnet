using Gauge.CSharp.Lib;

namespace ReferenceProject {
    public class ScreenshotWriter : ICustomScreenshotWriter {
        public string TakeScreenShot() {
            return "ReferenceProject-IDoNotExist.png";
        }
    }
}