///<reference path="angular.min.js" />
///<refrence path="Module.js">

app.service("ciService",['$http', function ($http) {

    this.CashIn = function (config) {

        var tid = $http.get("/MNepal/MNCashInOut/GetTraceId");//get the trace id from rest api
        tid.then(function (response) {
            config.tid = response.data;
            var request = $http({
                method: "post",
                url: "http://27.111.30.126/MNepal.WCF/cash/In",
                data: $.param(config),
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
                }
            });

            request.then(function (response) {
                alert("success");
            });
        })
        
       // return request;
    }

}]);

app.service("coService", ['$http', function ($http) {

    this.CashOut = function (config) {

        var request = $http({
            method: "post",
            url: "http://27.111.30.126/MNepal.WCF/cash/Out",
            data: $.param(config),
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
            }
        });

        request.then(function (response) {
            alert("success");
        });
        // return request;
    }

}]);