document.addEventListener('DOMContentLoaded', function () {
    var moneyInputs = document.querySelectorAll('.money-input');

    moneyInputs.forEach(function (input) {
        input.addEventListener('blur', function () {
            var value = parseFloat(input.value);

            if (!isNaN(value)) {
                input.value = value.toFixed(2);
            }
        });
    });
});