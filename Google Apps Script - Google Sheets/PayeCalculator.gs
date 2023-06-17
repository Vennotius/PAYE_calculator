/**
 * Calculate the net income after tax and UIF deductions.
 *
 * @param {Array.Array} bracketInfo - A 2D array where each sub-array is a tax bracket. Each tax bracket should have three elements: [threshold, upperLimit, percentage].
 * @param {number} maxUif - The maximum amount for UIF (Unemployment Insurance Fund) contributions.
 * @param {number} brutoMonthlyIncome - The gross monthly income.
 * @return {number} The net monthly income after tax and UIF deductions.
 * @customfunction
 */
function NETTO_AFTER_PAYE_AND_UIF(bracketInfo, maxUif, brutoMonthlyIncome) {
  if (isNaN(maxUif)) return 'maxUif is NaN';
  
  brutoMonthlyIncome = parseFloat(brutoMonthlyIncome);
  if (isNaN(brutoMonthlyIncome)) return 'brutoMonthlyIncome is NaN';
  var yearlyIncome = brutoMonthlyIncome * 12;
  var monthlyTax = 0;
  var remainingIncome = yearlyIncome;

  bracketInfo.forEach(function(row) {
    var threshold = parseFloat(row[0]);
    var upperLimit = parseFloat(row[1]);
    var percentage = parseFloat(row[2]);
    if (isNaN(threshold)) return 'threshold is NaN';
    if (isNaN(upperLimit)) return 'upperLimit is NaN';
    if (isNaN(percentage)) return 'percentage is NaN';


    var bracketWidth = upperLimit - threshold;
    var incomeInsideBracket = Math.min(bracketWidth, remainingIncome);

    if (incomeInsideBracket <= 0) return;

    monthlyTax += incomeInsideBracket * percentage;

    remainingIncome -= incomeInsideBracket;
  });

  monthlyTax /= 12;

  var uif = Math.min(brutoMonthlyIncome * 0.01, parseFloat(maxUif));

  var monthlyIncomeAfterTax = brutoMonthlyIncome - monthlyTax;
  
  return monthlyIncomeAfterTax - uif;
}

/**
 * Calculate the gross income needed to achieve a target net income, taking into account tax and UIF deductions.
 *
 * @param {Array.Array} bracketInfo - A 2D array where each sub-array is a tax bracket. Each tax bracket should have three elements: [threshold, upperLimit, percentage].
 * @param {number} maxUif - The maximum amount for UIF (Unemployment Insurance Fund) contributions.
 * @param {number} targetNetto - The target net income.
 * @return {number} The gross income needed to achieve the target net income.
 * @customfunction
 */
function BRUTO_FOR_TARGET_NETTO_AFTER_PAYE_AND_UIF(bracketInfo, maxUif, targetNetto) {
  targetNetto = parseFloat(targetNetto);
  var lowerBound = targetNetto;
  var upperBound = targetNetto * 2;
  var candidateTarget = 0;

  while (Math.abs(upperBound - lowerBound) > 0.0001) {
    var middle = (upperBound + lowerBound) / 2;
    var candidateNetto = NETTO_AFTER_PAYE_AND_UIF(bracketInfo, maxUif, middle);

    if (candidateNetto < targetNetto) {
      lowerBound = middle;
    } else if (candidateNetto > targetNetto) {
      upperBound = middle;
    } else {
      break;
    }

    candidateTarget = middle;
  }

  return Math.round(candidateTarget);
}
