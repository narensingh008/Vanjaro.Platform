﻿app.controller('setting_add', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    $scope.rid = $routeParams["rid"];
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        if ($scope.ui.data.RoleGroup.Value != null)
            $scope.ui.data.RoleGroup.Value = parseInt($scope.ui.data.RoleGroup.Value);
        if ($scope.ui.data.Status.Value != null)
            $scope.ui.data.Status.Value = parseInt($scope.ui.data.Status.Value);
        if ($scope.ui.data.SecurityMode.Value != null)
            $scope.ui.data.SecurityMode.Value = parseInt($scope.ui.data.SecurityMode.Value);
    };

    $scope.Click_AddGroup = function () {
        swal({
            title: "[LS:AddGroup]",
            type: "input",
            showCancelButton: true,
            closeOnConfirm: false,
            confirmButtonText: "[L:ConfirmOK]",
            cancelButtonText: "[L:ConfirmCancel]",
            inputPlaceholder: "[LS:EnterGroupName]"
        }, function (inputValue) {
            if (inputValue === false)
                return false;
            if (inputValue === "") {
                swal.showInputError("[LS:ErrorAddingGroup]");
                return false
            }

            $scope.ui.data.Working_RoleGroupDto.Options.Description = inputValue;
            $scope.ui.data.Working_RoleGroupDto.Options.Name = inputValue;
            common.webApi.post('rolegroup/SaveRoleGroup', '', $scope.ui.data.Working_RoleGroupDto.Options).then(function (Response) {
                if (Response.data.IsSuccess) {
                    $scope.ui.data.RoleGroup.Options = Response.data.Data.AllRoleGroup;
                    $scope.ui.data.Working_RoleDto.Options.groupId = Response.data.Data.FromRoleGroupInfo.id;
                    var ParentScope = parent.document.getElementById("iframe").contentWindow.angular;
                    if (ParentScope != undefined && ParentScope.element(".menuextension").scope() != undefined && ParentScope.element(".menuextension").scope().ui.data.RoleGroup != undefined) {
                        ParentScope.element(".menuextension").scope().ui.data.RoleGroup.Options = Response.data.Data.AllRoleGroup;
                        ParentScope.element(".menuextension").scope().$apply();
                    }
                    swal.close()
                    return true;
                }
                else {
                    window.parent.ShowNotification('[LS:Roles]', '[LS:error_AddGroup]', 'error');
                    return false;
                }
            });
        });
    };

    $scope.Click_SaveRole = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_add')) {
            $scope.isDisabled = true;
            common.webApi.post('role/saverole', '', $scope.ui.data.Working_RoleDto.Options).then(function (data) {
                if (data.data.IsSuccess) {
                    var ParentScope = parent.document.getElementById("iframe").contentWindow.angular;
                    if (ParentScope != undefined && ParentScope.element(".menuextension") != undefined && ParentScope.element(".menuextension").scope() != undefined && has(ParentScope.element(".menuextension").scope(), 'ui.data.RoleGroup')) {
                        $scope.ParentScope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
                        if (data.data.Data.Roles != null && data.data.Data.Roles.length > 0 && data.data.Data.Roles[0].GroupId != null) {
                            var option = { GroupId: data.data.Data.Roles[0].GroupId, Name: data.data.Data.Roles[0].GroupName };
                            if ($scope.ParentScope.FilterGroupOption != null)
                                $scope.ParentScope.filterGroup($scope.ParentScope.FilterGroupOption);
                            else
                                $scope.ParentScope.filterGroup(option);
                        }
                        $scope.ParentScope.ui.data.RoleGroup.Options = data.data.Data.RoleGroups;
                        $scope.ParentScope.$apply();
                    }
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                    window.parent.ShowNotification($scope.ui.data.Working_RoleDto.Options.name, $scope.rid > 0 ? '[LS:RoleUpdatedSuccess]' : '[LS:RoleCreatedSuccess]', 'success');
                }
                else {
                    window.parent.ShowNotification('[LS:Roles]', data.data.Message, 'error');
                }
                $scope.isDisabled = false;
            });
        }
    };

    has = function (obj, key) {
        return key.split(".").every(function (x) {
            if (typeof obj != "object" || obj === null || !x in obj || typeof obj[x] === "undefined")
                return false;
            obj = obj[x];
            return true;
        });
    };
});