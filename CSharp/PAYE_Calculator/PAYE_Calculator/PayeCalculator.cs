namespace PAYE_Calculator
{
    public static class PayeCalculator
    {
        private static readonly List<(decimal threshold, decimal upperlimit, decimal percentage)> _bracketsInfo = new() 
            {
                (0, 95750, 0 ),
                (95750, 237100, 0.18M ),
                (237100, 370500, 0.26M ),
                (370500, 512800, 0.31M ),
                (512800, 673000, 0.36M ),
                (673000, 857900, 0.39M ),
                (857900, 1817000, 0.41M ),
                (1817000, decimal.MaxValue, 0.45M ),
            };  // 2023-2024. cf https://www.sars.gov.za/tax-rates/income-tax/rates-of-tax-for-individuals/

        private static readonly decimal _maxUif = 117.12M;  // 2023-2024. cf https://www.sars.gov.za/types-of-tax/unemployment-insurance-fund/

        public static decimal GetNettoFromBruto(decimal brutoMonthlyIncome)
            => GetNettoFromBruto(_bracketsInfo, brutoMonthlyIncome, _maxUif);

        public static decimal GetBrutoForTargetNetto(decimal targetNetto)
            => GetBrutoForTargetNetto(_bracketsInfo, targetNetto, _maxUif);

        public static decimal GetNettoFromBruto(
            List<(decimal threshold, decimal upperLimit, decimal percentage)> bracketsInfo,
            decimal brutoMonthlyIncome,
            decimal maxUif)
        {
            // Get Tax For Monthly Income
            decimal monthlyTax = 0;
            decimal yearlyIncome = brutoMonthlyIncome * 12;
            decimal remainingIncome = yearlyIncome;
            decimal baseValue = 0;

            foreach (var (threshold, upperLimit, percentage) in bracketsInfo.OrderBy(b => b.threshold))
            {
                decimal bracketWidth = upperLimit - threshold;
                decimal incomeInsideBracket = remainingIncome > bracketWidth ? bracketWidth : remainingIncome;

                if (incomeInsideBracket <= 0) break;

                monthlyTax += incomeInsideBracket * percentage;
                remainingIncome -= incomeInsideBracket;

                baseValue += bracketWidth * percentage;
            }

            monthlyTax /= 12;

            // Get UIF
            decimal uif = Math.Min(brutoMonthlyIncome * 0.01M, maxUif);

            // Get Netto From Bruto
            decimal monthlyIncomeAfterTax = brutoMonthlyIncome - monthlyTax;

            return monthlyIncomeAfterTax - uif;
        }

        public static decimal GetBrutoForTargetNetto(
            List<(decimal threshold, decimal upperLimit, decimal percentage)> bracketsInfo,
            decimal targetNetto,
            decimal maxUif)
        {
            decimal lowerBound = targetNetto;
            decimal upperBound = targetNetto * 2;

            decimal candidateTarget = 0;

            while (Math.Abs(upperBound - lowerBound) > 0.001M)
            {
                decimal middle = (upperBound + lowerBound) / 2;

                decimal candidateNetto = GetNettoFromBruto(bracketsInfo, middle, maxUif);

                if (candidateNetto < targetNetto)
                    lowerBound = middle;
                else if (candidateNetto > targetNetto)
                    upperBound = middle;
                else
                    break;

                candidateTarget = middle;
            }

            return Math.Round(candidateTarget, 3);
        }
    }
}