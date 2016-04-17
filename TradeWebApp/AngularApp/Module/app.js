
(function () {

    var app = angular.module('trade', []);

    app.controller('DashboardController', ['$http', '$interval', function ($http, $interval) {
        var dashboard = this;
        dashboard.info = {};     

        $interval(function () {
            $http.get('../../api/dashboards').success(function (data) {
                dashboard.info = data;
            }).error(function (data) {
                dashboard.info = data;
            })
        }, 5000);

    }]);
    
})();