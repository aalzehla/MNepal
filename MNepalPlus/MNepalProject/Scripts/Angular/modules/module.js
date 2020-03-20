///<reference path="angular.min.js" />

var app = angular.module("MNepal", ['ngRoute', 'ngResource','ngCookies', 'mnController', 'mnServices','pagination','mnDirective']);

var mnController = angular.module("mnController", []);

var mnServices = angular.module("mnServices", []);

var pagingFilter = angular.module("pagination", []);

var mnDirective = angular.module('mnDirective', []);