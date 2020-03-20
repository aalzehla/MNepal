
/////<reference path="angular.min.js" />
///// <reference path="Module.js" />
///<refrence path="CrudServices.js"/>

mnController.controller('acc', ['$scope', 'accountService', function ($scope, accountService) {
    onLoad();

    function Default() {
        $scope.servicetype = "70";
        $scope.firstname = "";
        $scope.middlename = "";
        $scope.lastname = "";
        $scope.familyname = "";
        $scope.amount = "";
        $scope.umobile = "";
        $scope.dd = "";
        $scope.mm = "";
        $scope.yyyy = "";
        $scope.street = "";
        $scope.ward = "";
        $scope.district = "";
        $scope.zone = "";
        $scope.ivrlang="English"
        $scope.status = true;
    }

    function onLoad() {
        Default();
    }

    $scope.Register = function ()
    {
        var userdata = {
            sc: $scope.servicetype,
            firstname: $scope.firstname,
            middlename: $scope.middlename,
            lastname: $scope.lastname,
            familyname: $scope.familyname,
            amount: $scope.amount,
            umobile: $scope.umobile,
            dob: $scope.yyyy + '/' + $scope.mm + '/' + $scope.dd,
            street: $scope.street,
            ward: $scope.ward,
            district: $scope.district,
            zone: $scope.zone,
            ivrlang: $scope.ivrlang,
            photoid: ""
        };

        var register = accountService.register(userdata);
        register.then(function (response) {
            Message(response);
        }, function (error) {
            if (error.status == "500") {
                $scope.status = false;
                $scope.msg = "Something's wrong! It looks as though we've broken something on our system. Don't panic, we are fixing it! Please come back in a while";
                $scope.error = "true";
            }
        });

    }

    function Message(response) {// Message

        var data = response.data;
        var arr = data.split(',');
        var _succesStatus = arr[1];

        if (_succesStatus != "True") {
            $scope.status = false;
            $scope.msg = arr[0];
            $scope.error = "true";
        }
        else {
            $scope.status = false;
            $scope.msg = arr[0];
            $scope.error = "false";
        }
    }


}]);


