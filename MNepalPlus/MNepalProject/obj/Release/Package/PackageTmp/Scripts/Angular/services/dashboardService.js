///refrence path='angular.min.js'
///refrence path='module.js'

'use strict'

mnServices.factory('dashboardService', ['$http', function ($http) {

    function GetMyActivities()
    {
        var request = $http.get('/MNepal/DashBoard/GetMyActivities');
        return request;
    }

    var serviceFactory = {
        GetMyActivities:GetMyActivities
    };

    return serviceFactory;
}])