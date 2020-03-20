///refrence path='angular.min.js'
///refrence path='module.js'
///refrence path='dashboardService.js' //service

'use strict'

mnController.controller('dashboard', ['$scope', 'dashboardService', function ($scope, dashboardService) {

    onLoad();

    function onLoad()
    {
        var _data = dashboardService.GetMyActivities();
        _data.then(function (response) {
            if (response.status == 200) {
                var reqdData =response.data;
                if(reqdData)
                {
                    for(var item in reqdData)
                    {
                        $scope.items.push(reqdData[item]);
                    }
                }
            }
        }, function (error) {
            console.log(error);
        });
        paging();
    }

    /****************************************pagination***************************************/
    function paging() {
        //paging
        $scope.itemsPerPage = 5;
        $scope.currentPage = 0;
        $scope.items = [];//data list
        $scope.range = function () {
            var rangeSize = 5;
            var ps = [];
            var start;

            start = $scope.currentPage;
            if (start > $scope.pageCount() - rangeSize) {
                start = $scope.pageCount() - rangeSize + 1;
            }

            for (var i = start; i < start + rangeSize; i++) {
                if (i >= 0)
                    ps.push(i);
            }
            return ps;
        };

        $scope.prevPage = function () {
            if ($scope.currentPage > 0) {
                $scope.currentPage--;
            }
        };

        $scope.DisablePrevPage = function () {
            return $scope.currentPage === 0 ? "disabled" : "";
        };

        $scope.pageCount = function () {
            return Math.ceil($scope.items.length / $scope.itemsPerPage) - 1;
        };

        $scope.nextPage = function () {
            if ($scope.currentPage < $scope.pageCount()) {
                $scope.currentPage++;
            }
        };

        $scope.DisableNextPage = function () {
            return $scope.currentPage === $scope.pageCount() ? "disabled" : "";
        };

        $scope.setPage = function (n) {
            $scope.currentPage = n;
        };
    }

}]);