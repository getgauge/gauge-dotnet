using Gauge.CSharp.Lib;

namespace ReferenceDll {
    public class ScreenshotWriter : ICustomScreenshotWriter {
        public string TakeScreenShot() {
            return "ReferenceDll-IDoNotExist.png";
        }
    }
}