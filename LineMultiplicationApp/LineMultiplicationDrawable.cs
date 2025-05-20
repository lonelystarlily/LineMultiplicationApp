namespace LineMultiplicationApp
{
    public class LineMultiplicationDrawable : IDrawable
    {
        private readonly int num1 = 0, num2 = 0;
        private readonly float spacing = 40;
        private readonly int lineLengthFactor;

        private readonly Action<string>? resultCallback;

        public LineMultiplicationDrawable(int num1, int num2, Action<string>? resultCallback = null)
        {
            this.num1 = num1;
            this.num2 = num2;
            this.resultCallback = resultCallback;

            // 선 길이 계산용 계수 (자릿수 + 여유)
            int digits1 = num1.ToString().Length;
            int digits2 = num2.ToString().Length;
            int totalDigits = digits1 + digits2;

            // 자릿수가 작으면 더 길게
            if (totalDigits <= 3)
                lineLengthFactor = 7; // 길게
            else if (totalDigits <= 5)
                lineLengthFactor = 6;  // 중간
            else
                lineLengthFactor = totalDigits + 1;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {

            try
            {
                string str1 = num1.ToString();
                string str2 = num2.ToString();

                canvas.StrokeSize = 1;
                canvas.FontSize = 12;

                List<List<(PointF p1, PointF p2)>> redLinesByDigit = new();
                List<List<(PointF p1, PointF p2)>> blueLinesByDigit = new();

                // 빨간 선 (↗ 방향) - 첫 번째 숫자

                for (int i = 0; i < str1.Length; i++)
                {
                    int count = str1[i] - '0';
                    List<(PointF, PointF)> digitLines = new();

                    for (int k = 0; k < count; k++)
                    {
                        float offset = k * 4;
                        float x1 = i * spacing + offset;
                        float y1 = spacing * (str2.Length + 5);
                        float x2 = x1 + spacing * lineLengthFactor;
                        float y2 = y1 - spacing * lineLengthFactor;

                        canvas.StrokeColor = Colors.Red;
                        canvas.DrawLine(x1, y1, x2, y2);

                        digitLines.Add((new PointF(x1, y1), new PointF(x2, y2)));
                    }

                    redLinesByDigit.Add(digitLines);
                }


                // 파란 선 (↘ 방향) - 두 번째 숫자
                for (int j = 0; j < str2.Length; j++)
                {
                    int count = str2[j] - '0';
                    List<(PointF, PointF)> digitLines = new();

                    for (int k = 0; k < count; k++)
                    {
                        float offset = k * 4;
                        float x1 = j * spacing + offset;
                        float y1 = 0;
                        float x2 = x1 + spacing * lineLengthFactor;
                        float y2 = y1 + spacing * lineLengthFactor;

                        canvas.StrokeColor = Colors.Blue;
                        canvas.DrawLine(x1, y1, x2, y2);

                        digitLines.Add((new PointF(x1, y1), new PointF(x2, y2)));
                    }

                    blueLinesByDigit.Add(digitLines);
                }


                // 교점 표시 + 위치 기반 가중치(자릿수) 계산
                canvas.FillColor = Colors.Green;
                float maxY = spacing * (str2.Length + 5);
                float minY = 0;
                float verticalRange = maxY - minY;

                int totalValue = 0;

                for (int i = 0; i < redLinesByDigit.Count; i++) // ↗ 자릿수 (num1)
                {
                    int place1 = str1.Length - 1 - i;

                    foreach (var red in redLinesByDigit[i])
                    {
                        for (int j = 0; j < blueLinesByDigit.Count; j++) // ↘ 자릿수 (num2)
                        {
                            int place2 = str2.Length - 1 - j;

                            foreach (var blue in blueLinesByDigit[j])
                            {
                                if (TryGetLineIntersection(red.p1, red.p2, blue.p1, blue.p2, out PointF intersection))
                                {
                                    canvas.FillColor = Colors.Green;
                                    canvas.FillCircle(intersection.X, intersection.Y, 2);

                                    int power = place1 + place2;
                                    totalValue += (int)Math.Pow(10, power);
                                }
                            }
                        }
                    }
                }


                // 실제 곱셈 결과와 비교
                int actualProduct = num1 * num2;
                bool isCorrect = totalValue == actualProduct;

                // 결과 출력
                resultCallback?.Invoke(
                    $"시각적 계산값: {totalValue}\n" +
                    $"실제 곱셈값: {actualProduct}\n" +
                    $"일치 여부: {(isCorrect ? "일치 ✅" : "오류 ❌")}");


                Console.WriteLine("Draw completed without exception.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Draw Error] {ex.Message}\n{ex.StackTrace}");
            }
        }

        // 두 선분의 교차점 구하기
        private bool TryGetLineIntersection(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersection)
        {
            float A1 = p2.Y - p1.Y;
            float B1 = p1.X - p2.X;
            float C1 = A1 * p1.X + B1 * p1.Y;

            float A2 = p4.Y - p3.Y;
            float B2 = p3.X - p4.X;
            float C2 = A2 * p3.X + B2 * p3.Y;

            float delta = A1 * B2 - A2 * B1;
            if (Math.Abs(delta) < 0.001f)
            {
                intersection = default;
                return false; // 평행
            }

            float x = (B2 * C1 - B1 * C2) / delta;
            float y = (A1 * C2 - A2 * C1) / delta;
            intersection = new PointF(x, y);

            // 교차점이 두 선분 내에 있는지 확인
            if (IsBetween(p1, p2, intersection) && IsBetween(p3, p4, intersection))
                return true;

            return false;
        }
        private bool IsBetween(PointF a, PointF b, PointF c)
        {
            return Math.Min(a.X, b.X) - 0.1 <= c.X && c.X <= Math.Max(a.X, b.X) + 0.1 &&
                   Math.Min(a.Y, b.Y) - 0.1 <= c.Y && c.Y <= Math.Max(a.Y, b.Y) + 0.1;
        }
    }

}
