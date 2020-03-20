
function ToExcel(Id, IsTable,FileName) {
    var table = document.getElementById(Id).innerHTML;
    if (table == "") {
        return;
    }
    if (IsTable) {
        table = '<table>' + table + '</table>';
    }
    var blob = new Blob([table], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8"
    });
    saveAs(blob, FileName+'.xls');
}