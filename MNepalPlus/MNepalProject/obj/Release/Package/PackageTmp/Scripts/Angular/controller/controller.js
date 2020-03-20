//controller related to myApp

/////<reference path="angular.min.js" />
///// <reference path="Module.js" />
///<refrence path="CrudServices.js"/>


//CASH IN
app.controller('CashIn', ['$scope', 'ciService', function ($scope, ciService) {
    $scope.AMobileNo = "";
    $scope.UMobileNo = "";
    $scope.Amount = "";
    $scope.APin = "";
    $scope.sc = "";

 
    $scope.cashIn = function()
    {
        var cashIn = {
            amobile: $scope.AMobileNo,
            umobile:$scope.UMobileNo,
            amount:$scope.Amount,
            pin:$scope.APin,
            sc: $scope.sc,
            src:"http" //by default it is http
        }
        ciService.CashIn(cashIn);
    }

  }]);

//CASH OUT
app.controller('CashOut', ['$scope', 'coService', function ($scope, coService) {
    $scope.AMobileNo = "";
    $scope.UMobileNo = "";
    $scope.Amount = "";
    $scope.APin = "";

    $scope.cashOut =function() {

        var cashOut = {
            tid: "123",
            amobile: $scope.AMobileNo,
            umobile: $scope.UMobileNo,
            amount: $scope.Amount,
            pin: $scope.UPin,
            src: "http"
        }
        coService.CashOut(cashOut);
    }
   
}]);