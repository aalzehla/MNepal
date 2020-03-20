///refrence path='angular.min.js'
///refrence path='module.js'

'use strict'

mnServices.factory('tService', ['$http','$q', function ($http,$q) {
    
    /*    Start:Non-Finalcial Transaction  */

    function BalanceQuery(pin,userType) {
        
        var request = $http({
            method: 'get',          
            url: '/MNepal/Transaction/GetBalance?PIN=' + pin + '&uType=' + userType
        });
        return request;
    }

    function getBalanceFromDB()
    {
        var defered = $q.defer();
        var request = $http({
            method: "get",
            url: "/MNepal/Transaction/GetBalanceFromDatabase"
        });
        request.then(function (response) {
            defered.resolve(response.data)
        }, function (error) {
            defered.reject(error);
        });
        return defered.promise;
    }

    function MiniStatement(pin, userType) {

        var request = $http({
            method: 'get',
            url: '/MNepal/Transaction/GetMiniStatement?PIN=' + pin + '&uType=' + userType
        });
        return request;
    }

    /*--    END:Non-Finalcial Transaction  --*/


    /*    Start:Finalcial Transaction  */

    function FundTransfer(param)
    {
        var url = "/MNepal/Transaction/FundTransfer";
        var request = $http({
            method: 'post',
            url: url,
            data: param
        });

        return request;
    }

    function getCategory(_serviceCode)
    {
        var defered = $q.defer();
        var request = $http({
            method: 'get',
            url: "http://27.111.30.126/MNepal.WCF/query/merchant"
        });

        request.then(function (response) {

            var data = JSON.parse(response.data.d);
            if (_serviceCode == '31') {
                var _cat = data["utility"];
                defered.resolve(_cat)
            }
            else if (_serviceCode == '30') {
                var _cat = data["merchants"];
                defered.resolve(_cat);
            }
            return data;
        }, function (error) {
            defered.reject(error);
        });

        return defered.promise;
    }

    //
    function payment(data)
    {
        var defered = $q.defer();
        var url="/MNepal/Transaction/MNPayment";
        var request = $http(
            {
                method: "post",
                url: url,
                data: data
            });
        request.then(function () {
        })
    }

    /*    End:Finalcial Transaction  */

    var serviceFactory = {
        GetBalance: BalanceQuery, 
        MiniStatement: MiniStatement,
        getBalanceFromDB: getBalanceFromDB,

        //financial
        FundTransfer: FundTransfer,
        getCategory:getCategory,
        payment:payment
    }

    return serviceFactory;
}]);

