///refrence path='angular.min.js'
///refrence path='module.js'
///refrence path='TransactionService.js'

//tService===Transaction Service
'use strict'

mnDirective.directive('balance', function () {
    return {
        restrict: 'A',
        template:'Available Balance Rs. {{balance}}',
        controller: ['$scope', '$cookies', 'tService', function ($scope, $cookies, tService) {
            balanceOnLoad()
            function balanceOnLoad() {
                var result = tService.getBalanceFromDB();
                result.then(function (response) {
                    $scope.balance = response;
                }, function (error) {
                    console.log(error);
                })
            }
        }]
        //template: '<li data-ng-model="balance"><a href="" >{{balance}}</a></li>'
    }

    //return {
    //    restrict: 'A',
    //    scope: {
    //        bal: '@'
    //    },
    //    template: 'balance Rs. {{bal}}',
    //    //controller: ['$scope', '$cookies', 'tService', function ($scope, $cookies, tService) {
    //    //    balanceOnLoad()
    //    //    function balanceOnLoad() {
    //    //        var result = tService.getBalanceFromDB();
    //    //        result.then(function (response) {
    //    //            $scope.balance = response;
    //    //        }, function (error) {
    //    //            console.log(error);
    //    //        })
    //    //    }
    //    //}]
    //}
})

