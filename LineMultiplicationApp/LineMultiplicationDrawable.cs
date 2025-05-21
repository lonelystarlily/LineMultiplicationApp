using Microsoft.Maui.Controls.PlatformConfiguration;

namespace LineMultiplicationApp
{
    public class LineMultiplicationDrawable : IDrawable
    {
        // 숫자 두 개를 저장
        private readonly int num1, num2;


        // (붙은 선 간격)
        private readonly float spacing2 = 10;

        // 선 길이 조정용 계수 (자릿수에 따라 달라짐)
        private readonly int lineLengthFactor;

        // 계산 결과를 반환하는 콜백 (결과를 외부에서 처리 가능)
        private readonly Action<string>? resultCallback;

        public LineMultiplicationDrawable(int num1, int num2, Action<string>? resultCallback = null)
        {
            this.num1 = num1;
            this.num2 = num2;
            this.resultCallback = resultCallback;

            // 숫자의 자릿수 계산
            int digits1 = num1.ToString().Length;
            int digits2 = num2.ToString().Length;
            int totalDigits = digits1 + digits2;

            // 자릿수에 따라 선 길이 조정 (작을수록 더 길게 설정)
            lineLengthFactor = totalDigits <= 3 ? 7 : totalDigits <= 5 ? 6 : totalDigits + 1;
        }

        public float Scale { get; set; } = 1.0f;
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float offsetXByDevice = 0;
            float offsetYByDevice = 0;
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                offsetXByDevice = 400;
                offsetYByDevice = 100;
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                offsetXByDevice = 100;
                offsetYByDevice = 100;
            }

            try
            {
                canvas.SaveState();
                canvas.Scale(Scale, Scale);

                string str1 = num1.ToString();
                string str2 = num2.ToString();

                canvas.StrokeSize = 1;
                canvas.FontSize = 12;

                List<List<(PointF p1, PointF p2)>> redLinesByDigit = new();
                List<List<(PointF p1, PointF p2)>> blueLinesByDigit = new();

                float spacing = dirtyRect.Width / (Math.Max(str1.Length, str2.Length) + 5);

                float redLineLength = (Math.Max(str1.Length, str2.Length) + lineLengthFactor + 3) * spacing;

                // 첫 번째 숫자의 빨간 선 (↗ 방향)
                for (int i = 0; i < str1.Length; i++)
                {
                    int count = str1[i] - '0';
                    List<(PointF, PointF)> digitLines = new();

                    for (int k = 0; k < count; k++)
                    {
                        float offset = k * spacing2;
                        float x1 = i * spacing + offset + offsetXByDevice;
                        float y1 = spacing * (str2.Length + 7) * 0.5f + offsetYByDevice;
                        float x2 = x1 + redLineLength;
                        float y2 = y1 - redLineLength;

                        canvas.StrokeColor = Colors.Red;
                        canvas.DrawLine(x1, y1, x2, y2);

                        digitLines.Add((new PointF(x1, y1), new PointF(x2, y2)));
                    }

                    redLinesByDigit.Add(digitLines);
                }

                float maxX = (Math.Max(str1.Length, str2.Length) + lineLengthFactor + 4) * spacing;

                // 두 번째 숫자의 파란 선 (↘ 방향)
                for (int j = 0; j < str2.Length; j++)
                {
                    int count = str2[j] - '0';
                    List<(PointF, PointF)> digitLines = new();

                    for (int k = 0; k < count; k++)
                    {
                        float offset = k * spacing2;
                        float x1 = j * spacing + offset + offsetXByDevice;
                        float y1 = offsetYByDevice;
                        float x2 = x1 + spacing * (lineLengthFactor + 2); // 파란 선 길이 연장
                        float y2 = y1 + spacing * (lineLengthFactor + 2); // 모든 빨간 선과 교차하도록 설정

                        if (x2 > maxX)
                        {
                            float overflow = x2 - maxX;
                            x1 -= overflow * 0.75f;
                            x2 -= overflow * 0.75f;
                        }

                        canvas.StrokeColor = Colors.Blue;
                        canvas.DrawLine(x1, y1, x2, y2);

                        digitLines.Add((new PointF(x1, y1), new PointF(x2, y2)));
                    }

                    blueLinesByDigit.Add(digitLines);
                }

                canvas.FillColor = Colors.Green;
                int totalValue = 0;

                // 교차점 계산 및 곱셈 값 생성
                for (int i = 0; i < redLinesByDigit.Count; i++)
                {
                    int place1 = str1.Length - 1 - i;

                    foreach (var red in redLinesByDigit[i])
                    {
                        for (int l = 0; l < blueLinesByDigit.Count; l++)
                        {
                            int place2 = str2.Length - 1 - l;

                            foreach (var blue in blueLinesByDigit[l])
                            {
                                if (TryGetLineIntersection(red.p1, red.p2, blue.p1, blue.p2, out PointF intersection))
                                {
                                    canvas.FillCircle(intersection.X, intersection.Y, 2);

                                    int power = place1 + place2;
                                    totalValue += (int)Math.Pow(10, power);
                                }
                            }
                        }
                    }
                }

                int actualProduct = num1 * num2;
                bool isCorrect = totalValue == actualProduct;

                resultCallback?.Invoke(
                    $"시각적 계산값: {totalValue}\n" +
                    $"실제 곱셈값: {actualProduct}\n");

                canvas.RestoreState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류: {ex.Message}");
            }
        }





        // 두 선분이 교차하는지 확인하는 함수
        private bool TryGetLineIntersection(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersection)
        {
            // 두 직선의 방정식 계산
            float A1 = p2.Y - p1.Y;
            float B1 = p1.X - p2.X;
            float C1 = A1 * p1.X + B1 * p1.Y;

            float A2 = p4.Y - p3.Y;
            float B2 = p3.X - p4.X;
            float C2 = A2 * p3.X + B2 * p3.Y;

            float delta = A1 * B2 - A2 * B1;
            if (Math.Abs(delta) < 0.0001f)
            {
                intersection = default;
                return false; // 평행함
            }

            // 교차점 좌표 계산
            float x = (B2 * C1 - B1 * C2) / delta;
            float y = (A1 * C2 - A2 * C1) / delta;
            intersection = new PointF(x, y);

            return IsBetween(p1, p2, intersection) && IsBetween(p3, p4, intersection);
        }

        // 점이 두 선분 사이에 있는지 확인하는 함수
        private bool IsBetween(PointF a, PointF b, PointF c)
        {
            return Math.Min(a.X, b.X) <= c.X && c.X <= Math.Max(a.X, b.X) &&
                   Math.Min(a.Y, b.Y) <= c.Y && c.Y <= Math.Max(a.Y, b.Y);
        }
    }
}
