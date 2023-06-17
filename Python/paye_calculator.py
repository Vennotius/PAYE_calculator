import numpy as np

class PayeCalculator:
    _brackets_info = [
        (0, 95750, 0),
        (95750, 237100, 0.18),
        (237100, 370500, 0.26),
        (370500, 512800, 0.31),
        (512800, 673000, 0.36),
        (673000, 857900, 0.39),
        (857900, 1817000, 0.41),
        (1817000, np.inf, 0.45),
    ]
    
    _max_uif = 177.12
    
    @classmethod
    def get_netto_from_bruto(cls, bruto_monthly_income):
        return cls._netto_after_paye_and_uif(cls._brackets_info, cls._max_uif, bruto_monthly_income)

    @classmethod
    def get_bruto_for_target_netto(cls, target_netto):
        return cls._get_bruto_for_target_netto(cls._brackets_info, cls._max_uif, target_netto)

    @staticmethod
    def _netto_after_paye_and_uif(bracket_info, max_uif, bruto_monthly_income):
        bruto_monthly_income = float(bruto_monthly_income)
        yearly_income = bruto_monthly_income * 12
        monthly_tax = 0
        remaining_income = yearly_income

        for row in bracket_info:
            threshold, upper_limit, percentage = map(float, row)

            bracket_width = upper_limit - threshold
            income_inside_bracket = min(bracket_width, remaining_income)

            if income_inside_bracket <= 0:
                break

            monthly_tax += income_inside_bracket * percentage
            remaining_income -= income_inside_bracket

        monthly_tax /= 12
        uif = min(bruto_monthly_income * 0.01, float(max_uif))

        monthly_income_after_tax = bruto_monthly_income - monthly_tax
        return monthly_income_after_tax - uif

    @staticmethod
    def _get_bruto_for_target_netto(bracket_info, max_uif, target_netto):
        target_netto = float(target_netto)
        lower_bound = target_netto
        upper_bound = target_netto * 2
        candidate_target = 0

        while np.abs(upper_bound - lower_bound) > 0.0001:
            middle = (upper_bound + lower_bound) / 2
            candidate_netto = PayeCalculator._netto_after_paye_and_uif(bracket_info, max_uif, middle)

            if candidate_netto < target_netto:
                lower_bound = middle
            elif candidate_netto > target_netto:
                upper_bound = middle
            else:
                break

            candidate_target = middle

        return np.round(candidate_target)


# Quick eyeball test:
# for bruto in range(0, 30001, 500):
#     pc = PayeCalculator()
#     netto = pc.get_netto_from_bruto(bruto)
#     bruto_again = pc.get_bruto_for_target_netto(netto)
#     print(f"{bruto} -> {netto:.2f} -> {bruto_again:.2f}")