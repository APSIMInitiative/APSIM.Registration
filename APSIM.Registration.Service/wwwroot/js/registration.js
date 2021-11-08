$(document).ready(function () {
    $('#dropDownLicenseType').change(onLicenseTypeChanged);
    $('#Product').change(onProductChanged);
    onLicenseTypeChanged();
    updateLicense();
});

function onLicenseTypeChanged() {
    // The options for commercial users should only be visible
    // when the commercial license type radio button is checked.
    var dropDown = $('#dropDownLicenseType')[0];
    if (dropDown != null) {
        if (dropDown.selectedIndex == 1) {
            $('.commercialInput').each(function () {
                $(this).show();
            });
        } else {
            $('.commercialInput').each(function () {
                $(this).hide();
            });
        }
        updateLicense();
    }
}

function onProductChanged() {
    // User has changed the selected product. Need to update license
    // info and hide the row containing version selection if apsoil
    // is selected.
    updateLicense();
    var product = $('#Product').find('option:selected').text();
    if (product == 'APSIM') {
        $('#versionRow').show();
    } else {
        $('#versionRow').hide();
    }
}

function updateLicense() {
    var product = $('#Product').find('option:selected').text();
    var src = '';
    if (product == 'APSIM') {
        if (!$('#radioCom').prop('checked'))
            src = 'APSIM_NonCommercial_RD_licence.htm';
        else {
            src = 'APSIM_Commercial_Licence.htm';
        }
    } else {
        src = 'OtherDisclaimer.html';
    }
    $('td#Terms').html(`<iframe height="300px" width="700px" src="${src}" />`);
    console.log(product);
}
