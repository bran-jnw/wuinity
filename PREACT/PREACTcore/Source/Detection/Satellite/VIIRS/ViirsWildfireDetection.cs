using static System.Math;

namespace PREACT.Detection
{
    public enum FireConfidence
    {
        None = 0,
        Low = 1,
        Nominal = 2,
        High = 3
    }

    public sealed class ViirsAfResult
    {
        public FireConfidence[,] Confidence { get; private set; }
        public float[,] FrpMw { get; private set; }

        public ViirsAfResult(int rows, int cols, bool includeFrp)
        {
            Confidence = new FireConfidence[rows, cols];
            FrpMw = includeFrp ? new float[rows, cols] : null;
        }
    }

    public sealed class ViirsAfConfig
    {
        public float Bt4MinK { get; set; } = 310f;
        public float DtMinK { get; set; } = 10f;

        public int ContextRadius { get; set; } = 10;

        public float N1 { get; set; } = 3.0f;
        public float C1 { get; set; } = 3.0f;

        public float N2 { get; set; } = 3.0f;
        public float C2 { get; set; } = 3.0f;
    }

    public sealed class ViirsAfDetector
    {
        private readonly ViirsAfConfig _cfg;

        public ViirsAfDetector(ViirsAfConfig cfg)
        {
            _cfg = cfg;
        }

        public ViirsAfResult Run(float[,] bt4, float[,] bt11, float[,] reflM5, float[,] reflM7, float[,] reflM11, float[,] reflM16, bool[,] isCloud, bool[,] isWater, float[,] solarZenithDeg, float[,] viewZenithDeg, bool computeFrp)
        {
            int rows = bt4.GetLength(0);
            int cols = bt4.GetLength(1);

            ViirsAfResult result = new ViirsAfResult(rows, cols, computeFrp);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (!IsCandidate(r, c, bt4, bt11, isCloud, isWater))
                        continue;

                    float score1;
                    float score2;

                    if (!PassesContextTests(r, c, bt4, bt11, isCloud, out score1, out score2))
                        continue;

                    if (IsFalseAlarm(r, c, reflM5, reflM7, reflM11, reflM16,
                                     solarZenithDeg, viewZenithDeg, isWater))
                        continue;

                    FireConfidence conf = ClassifyConfidence(score1, score2);
                    result.Confidence[r, c] = conf;

                    if (computeFrp && result.FrpMw != null)
                    {
                        result.FrpMw[r, c] = EstimateFrp(bt4[r, c], viewZenithDeg[r, c]);
                    }
                }
            }

            return result;
        }

        private bool IsCandidate(int r, int c, float[,] bt4, float[,] bt11, bool[,] isCloud, bool[,] isWater)
        {
            if (isCloud[r, c])
                return false;

            float t4 = bt4[r, c];
            float t11 = bt11[r, c];

            if (float.IsNaN(t4) || float.IsNaN(t11))
                return false;

            if (t4 < _cfg.Bt4MinK)
                return false;

            float dt = t4 - t11;
            if (dt < _cfg.DtMinK)
                return false;

            return true;
        }

        private bool PassesContextTests(int r, int c, float[,] bt4, float[,] bt11, bool[,] isCloud, out float score1, out float score2)
        {
            score1 = 0f;
            score2 = 0f;

            int rows = bt4.GetLength(0);
            int cols = bt4.GetLength(1);
            int R = _cfg.ContextRadius;

            double sumT4 = 0.0;
            double sumT4Sq = 0.0;
            double sumDt = 0.0;
            double sumDtSq = 0.0;
            int n = 0;

            float t4Pix = bt4[r, c];
            float t11Pix = bt11[r, c];
            float dtPix = t4Pix - t11Pix;

            for (int rr = Max(0, r - R); rr <= Min(rows - 1, r + R); rr++)
            {
                for (int cc = Max(0, c - R); cc <= Min(cols - 1, c + R); cc++)
                {
                    if (rr == r && cc == c)
                        continue;
                    if (isCloud[rr, cc])
                        continue;

                    float t4 = bt4[rr, cc];
                    float t11 = bt11[rr, cc];

                    if (float.IsNaN(t4) || float.IsNaN(t11))
                        continue;

                    float dt = t4 - t11;

                    sumT4 += t4;
                    sumT4Sq += t4 * t4;
                    sumDt += dt;
                    sumDtSq += dt * dt;
                    n++;
                }
            }

            if (n < 10)
                return false;

            double muT4 = sumT4 / n;
            double sigmaT4 = Sqrt(Max(sumT4Sq / n - muT4 * muT4, 1e-6));

            double muDt = sumDt / n;
            double sigmaDt = Sqrt(Max(sumDtSq / n - muDt * muDt, 1e-6));

            score1 = (float)((t4Pix - muT4 - _cfg.C1) / sigmaT4);
            score2 = (float)((dtPix - muDt - _cfg.C2) / sigmaDt);

            bool pass1 = score1 > _cfg.N1;
            bool pass2 = score2 > _cfg.N2;

            return pass1 || pass2;
        }

        private bool IsFalseAlarm(int r, int c, float[,] reflM5, float[,] reflM7, float[,] reflM11, float[,] reflM16, float[,] solarZenithDeg, float[,] viewZenithDeg, bool[,] isWater)
        {
            float sza = solarZenithDeg[r, c];
            float rhoM5 = reflM5[r, c];

            if (sza < 60f && rhoM5 > 0.3f)
                return true;

            return false;
        }

        private FireConfidence ClassifyConfidence(float score1, float score2)
        {
            float s = score1 > score2 ? score1 : score2;

            if (s < 3f)
                return FireConfidence.Low;
            if (s < 6f)
                return FireConfidence.Nominal;

            return FireConfidence.High;
        }

        private float EstimateFrp(float bt4K, float viewZenithDeg)
        {
            // Replace with ATBD FRP formula using radiance and pixel area.
            return 0f;
        }
    }
}
