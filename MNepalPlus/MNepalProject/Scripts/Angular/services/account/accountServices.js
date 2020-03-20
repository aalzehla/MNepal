/////refrence path="angular.min.js"
///refrence path="angular-route.min.js"
///refrence path="angular-resource.min.js"
///refrence path="module.js"


'use strict'

mnServices.factory('accountService', ['$http', function ($http) {

    function RegisterUser(userdata) {
        var request = $http({
            method: 'post',
            url: '../Account/Register',
            data:userdata
        })
        return request;
    
    }
    var serviceFactory = {
        register: RegisterUser,
    }

    return serviceFactory;
}]);