///refrence path='angular.min.js'
///refrence path='module.js'
///refrence path='TransactionService.js'

//tService===Transaction Service
'use strict'

mnController.controller('balance', ['$scope', '$cookies', 'tService', function ($scope, $cookies, tService) {

    balanceOnLoad()
    function balanceOnLoad() {
        var result = tService.getBalanceFromDB();
        result.then(function (response) {
            $scope.balance = response;
        }, function (error) {
            console.log(error);
        })
    }
}]);

//Financial Transaction

mnController.controller('FundTransfer', ['$scope', 'tService', function ($scope, tService) {

    onLoad();

    function Default() {
        $scope.mthd = false;
        $scope.error = "";
        $scope.msg = "";
        $scope.PIN = "";
        $scope.tType = '00';
        $scope.rMobile = "";
        $scope.amount = "";
        $scope.remark = "";
        $scope.status = true;
        $scope.values =
            [
                { id: '00', label: 'Wallet to Wallet' },
                { id: '01', label: 'Wallet to Bank' },
                { id: '10', label: 'Bank to Wallet' },
                { id: '11', label: 'Bank to Bank' }
            ];
    }
  
    function onLoad()
    { 
        Default();
    }

    $scope.fTransfer=function()
    {
        var parameters = {
            sc: $scope.tType,
            da: $scope.rMobile,//destination address
            amount: $scope.amount,
            pin:$scope.PIN,
            note: $scope.remark,
            sourcechannel:'http'
        }
        var result = tService.FundTransfer(parameters);
        result.then(function (response) {
            Message(response);
        }, function (error) {
            if(error.status=="500")
            {
                $scope.status = false;
                $scope.msg ="Something's wrong! It looks as though we've broken something on our system. Don't panic, we are fixing it! Please come back in a while";
                $scope.error = "true";
            }
        })
       
    }

    function Message(response) {
        
        var data = response.data;
        var array = data.split(',');

        if (data.indexOf("OK") != -1)
        {
            var _succMsg = "";
            for(var i=1;i<array.length;i++)
            {
                _succMsg += array[i];
            }
            console.log(_succMsg);
            var msg = _succMsg.replace(/{|d|:|}|"|'/g, "");
            
            $scope.status = false;
            $scope.msg ='Success!!!!' + "  " + msg;
            $scope.error = "false";
            document.getElementById('bal').innerHTML = msg;
        }
        else {
            var msg = array[1];
            var _errorMsg = JSON.parse(msg).d;
            $scope.status = false;
            $scope.msg = _errorMsg;
            $scope.error = "true";
        }
    }

    $scope.pinMethod=function()
    {
        if ($scope.chkmethod)
        {
            $scope.mthd = true;
            $scope.PIN = "";
        }
        else
        {
            $scope.mthd = false;
        }
    }

}])

mnController.controller('BalanceQuery', ['$scope', 'tService', function ($scope, tService) {

    onLoad();

    function Default() {
        $scope.error = "";
        $scope.msg = "";
        $scope.PIN = "";
        $scope.status = true;
        $scope.uType="20"
    }

    function onLoad() {
        Default();
    }

    $scope.BalanceQuery = function () {
        var balance = tService.GetBalance($scope.PIN,$scope.uType);
        balance.then(function (response) {
            Message(response);
        }, function (error) {
            if (error.status == "500") {
                $scope.status = false;
                $scope.msg = "Something's wrong! It looks as though we've broken something on our system. Don't panic, we are fixing it! Please come back in a while";
                $scope.error = "true";
            }
        })
    }

    function Message(response) {//Message
       
        var data = response.data;
        var arr = data.split(',');

        //Actual Balance=4,821.00

        var msg = arr[1].split(':');//will give [" ReasonPhrase", " 'Available Balance Rs. 4"]
        //var _remaining = arr[2]//will give 821.00
        
        if (response.status != "200") {
            $scope.status = false;
            $scope.msg = msg[1];
            $scope.error = "true";
        }
        else {
            $scope.status = false;
            $scope.msg = msg[1]+','+arr[2];
            $scope.error = "false";
        }
    }

}])

mnController.controller('MiniStatement', ['$scope', 'tService', function ($scope, tService) {

    onLoad();

    function Default() {
        $scope.error = "";
        $scope.msg = "";
        $scope.PIN = "";
        $scope.status = true;
        $scope.uType = "21"
    }

    function onLoad() {
        Default();
    }

    $scope.mStatement = function () {
        var ms = tService.MiniStatement($scope.PIN, $scope.uType);
        ms.then(function (response) {
            Message(response);
        }, function (error) {
            if (error.status == "500") {
                $scope.status = false;
                $scope.msg = "Something's wrong! It looks as though we've broken something on our system. Don't panic, we are fixing it! Please come back in a while";
                $scope.error = "true";
            }
        })

    }

    function Message(response) {//Message
        if (response.status != "200") {
            $scope.status = false;
            $scope.msg = response.data.d;
            $scope.error = "true";
        }
        else {
            $scope.status = false;
            $scope.msg = response.data.d;
            $scope.error = "false";
        }
    }

}])

mnController.controller('UtilitiyPayment', ['$scope', 'tService', function ($scope, tService) {
    var sc = '31';

    onLoad();
    function onLoad()
    {
        getCategory();
        _default();
    }

    function _default()
    {
        $scope.ok = false;
        $scope.amount = "";
    }

    //Dropdown  : Category
    function getCategory()
    {
        var result = tService.getCategory(sc);
        result.then(function (response) {
            $scope.category = response;
            $scope._cat = response[0]//default catergorin category dropdown

            var item = $scope._cat;
            merchantDropDown(item);//fill drop merchant dropwon according to category
        }, function (error) {
            console.log(error);
            $scope.message = error.status;
        })
    }

    //Dropdown: Merchant
    function merchantDropDown(item)//functino for both merchant and denomination dropdown
    {
        //dropdown merchant default value
        $scope.merchant = item.merchants;//the default merchantlist according to default category
        $scope._merch = $scope.merchant[0]; //or response[0].merchants //for

        //dropdown denomination according to merchant Dropdown
        $scope.onChangeMerchantDropDown($scope._merch);
    }

    //Onchange event for Category Dropdown
    $scope.getMerchantsByCatID=function(item)
    {
        merchantDropDown(item);     
    }

    //Onchange event for Merchant Dropdwon
    //fill the denomiation dropdwon if denomiation available
    $scope.onChangeMerchantDropDown =function (item)
    {
        if (item.hasOwnProperty('denomiation')) {
            $scope.denomination = $scope._merch.denomiation;//Or console.log($scope.merchant[0]["denomiation"])
            $scope._deno = $scope.denomination[0];
            $scope.amount = $scope._deno;
            $scope.ok = true;
        }
        else {
            $scope.denomination = [];
            _default();
        }
    }

    //Onchange event for denomiation dropdown
    $scope.denomin = function (deno) {
        $scope.amount = deno;
        $scope.ok = true;
    }

    $scope.pinMethod = function () {
        if ($scope.chkPin) {
            $scope.pin = "";
        }
    }
    /*   ---Method:Payment  ----*/

    $scope.payment=function()
    {
        var _param = {
            prod:$scope._merch.mid,
            sc: sc,
            amount: $scope.amount,
            pin: $scope.pin,
            note: $scope.remark,
            src: 'http',
        }

        var result = tService.payment(_param);
    }

}]);

mnController.controller('MerchantPayment', ['$scope', 'tService', function ($scope, tService) {
    var sc='30'
    onLoad();
    function onLoad() {
        getCategory();
        _default();
    }

    function _default() {
        $scope.ok = false;
        $scope.amount = "";
    }

    //Dropdown  : Category
    function getCategory() {
        var result = tService.getCategory(sc);
        result.then(function (response) {
            $scope.category = response;
            $scope._cat = response[0]//default catergorin category dropdown

            var item = $scope._cat;
            merchantDropDown(item);//fill drop merchant dropwon according to category
        }, function (error) {
            console.log(error);
            $scope.message = error.status;
        })
    }

    //Dropdown: Merchant
    function merchantDropDown(item)//functino for both merchant and denomination dropdown
    {
        //dropdown merchant default value
        $scope.merchant = item.merchants;//the default merchantlist according to default category
        $scope._merch = $scope.merchant[0]; //or response[0].merchants //for

        //dropdown denomination according to merchant Dropdown
        $scope.onChangeMerchantDropDown($scope._merch);
    }

    //Onchange event for Category Dropdown
    $scope.getMerchantsByCatID = function (item) {
        merchantDropDown(item);
    }

    //Onchange event for Merchant Dropdwon
    //fill the denomiation dropdwon if denomiation available
    $scope.onChangeMerchantDropDown = function (item) {
        if (item.hasOwnProperty('denomiation')) {
            $scope.denomination = $scope._merch.denomiation;//Or console.log($scope.merchant[0]["denomiation"])
            $scope._deno = $scope.denomination[0];
            $scope.amount = $scope._deno;
            $scope.ok = true;
        }
        else {
            $scope.denomination = [];
            _default();
        }
    }

    //Onchange event for denomiation dropdown
    $scope.denomin = function (deno) {
        $scope.amount = deno;
        $scope.ok = true;
    }

    $scope.pinMethod = function () {
        if ($scope.chkPin) {
            $scope.pin = "";
        }
    }


    /*  ----Method:Payment  ----*/

    $scope.Payment = function () {
        var _param = {
            prod:"",
            vid: $scope._merch.mid,
            sc: sc,
            amount: $scope.amount,
            pin: $scope.pin,
            note: $scope.remark,
            src: 'http',
        }

        var result = tService.payment(_param);
    }
}]);
