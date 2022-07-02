class PermissionsManager():
    @staticmethod
    def intersection(lst1, lst2):
        lst3 = [value for value in lst1 if value in lst2]
        return lst3

    @staticmethod
    def validate_role_claim_permission(user_role, valid_roles_list):
        '''
        Returns true if the user has a valid role
        '''
        if (valid_roles_list is None or not valid_roles_list):
            return True
        elif (valid_roles_list and user_role and user_role in valid_roles_list):
            return True
        else:
            return False

    @staticmethod
    def validate_user_scopes_permission_all(user_scopes, valid_scopes_list):
        '''
        Returns true if the user has valid scopes
        '''
        if (valid_scopes_list is None or not valid_scopes_list):
            return True
        elif (valid_scopes_list and user_scopes):
            return len(PermissionsManager.intersection(user_scopes,valid_scopes_list)) == len(valid_scopes_list)
        else:
            return False