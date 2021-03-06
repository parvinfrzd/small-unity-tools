using System.Threading;

using UnityEngine;

namespace WC {
    public static class TextureScale {
        public class ThreadData {
            public int start;
            public int end;
            public ThreadData(int s, int e) {
                start = s;
                end = e;
            }
        }

        private static Color[] _texColors;
        private static Color[] _newColors;
        private static int _width;
        private static float _ratioX;
        private static float _ratioY;
        private static int _newWidth;
        private static int _finishCount;
        private static Mutex _mutex;

        public static void Point(Texture2D tex, int newWidth, int newHeight) {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight) {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear) {
            _texColors = tex.GetPixels();
            _newColors = new Color[newWidth * newHeight];
            if (useBilinear) {
                _ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                _ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else {
                _ratioX = ((float)tex.width) / newWidth;
                _ratioY = ((float)tex.height) / newHeight;
            }
            _width = tex.width;
            _newWidth = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            _finishCount = 0;
            if (_mutex == null) {
                _mutex = new Mutex(false);
            }
            if (cores > 1) {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++) {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear) {
                    BilinearScale(threadData);
                }
                else {
                    PointScale(threadData);
                }
                while (_finishCount < cores) {
                    Thread.Sleep(1);
                }
            }
            else {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear) {
                    BilinearScale(threadData);
                }
                else {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(_newColors);
            tex.Apply();

            _texColors = null;
            _newColors = null;
        }

        public static void BilinearScale(System.Object obj) {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++) {
                int yFloor = (int)Mathf.Floor(y * _ratioY);
                var y1 = yFloor * _width;
                var y2 = (yFloor + 1) * _width;
                var yw = y * _newWidth;

                for (var x = 0; x < _newWidth; x++) {
                    int xFloor = (int)Mathf.Floor(x * _ratioX);
                    var xLerp = x * _ratioX - xFloor;
                    _newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(_texColors[y1 + xFloor], _texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(_texColors[y2 + xFloor], _texColors[y2 + xFloor + 1], xLerp),
                                                           y * _ratioY - yFloor);
                }
            }

            _mutex.WaitOne();
            _finishCount++;
            _mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj) {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++) {
                var thisY = (int)(_ratioY * y) * _width;
                var yw = y * _newWidth;
                for (var x = 0; x < _newWidth; x++) {
                    _newColors[yw + x] = _texColors[(int)(thisY + _ratioX * x)];
                }
            }

            _mutex.WaitOne();
            _finishCount++;
            _mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value) {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }
    }
}
