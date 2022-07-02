from rest_framework import viewsets, status, permissions
from rest_framework.decorators import api_view
from rest_framework.views import APIView
from rest_framework.response import Response
from django.core.paginator import Paginator
from NewsServiceApp.ViewModels.serializers.setting_serializer import SettingSerializer
from NewsServiceApp.models.setting_model import Setting
from NewsServiceApp.services.permissions_manager import PermissionsManager
import json


#class SettingViewSet(viewsets.ModelViewSet):
#    """
#    API endpoint that allows settings to be viewed or edited.
#    """
#    queryset = Setting.objects.all().order_by('Key')
#    serializer_class = SettingSerializer

"""
    This class contains settings REST getter methods
    learn django:
    https://docs.microsoft.com/en-us/visualstudio/python/learn-django-in-visual-studio-step-02-create-an-app?view=vs-2022
    https://www.django-rest-framework.org/tutorial/quickstart/
    https://docs.djangoproject.com/en/4.0/ref/models/instances/#django.db.models.Model
    https://docs.djangoproject.com/en/4.0/topics/db/search/
    https://www.django-rest-framework.org/tutorial/2-requests-and-responses/#request-objects
    https://www.django-rest-framework.org/tutorial/3-class-based-views/
    https://blog.logrocket.com/django-rest-framework-create-api/
    https://www.w3schools.com/django/django_queryset_filter.php
"""
class SettingsListApiView(APIView):

    """
    Get all settings.
    """
    def get(self, request, format=None):
        userScopes = None if request.headers.get('UserClaims') is None else json.loads(request.headers.get('UserClaims'))
        if (PermissionsManager.validate_role_claim_permission(request.headers.get('UserRole'), ['Admin'])
            and PermissionsManager.validate_user_scopes_permission_all(userScopes, ['newsblog.admin'])):
            sortByField = 'Key' if request.GET.get('sortBy') is None else request.GET.get('sortBy')
            sortByOrder = 'asc' if request.GET.get('sortDirection') is None else request.GET.get('sortDirection')
            sortByField = sortByField if sortByOrder == 'asc' else '-' + sortByField
            queryset = Setting.objects.all().order_by(sortByField)
            search_text = request.GET.get('searchText', None)
            if search_text is not None:
                queryset = queryset.filter(Q(Key__contains=search_text) | Q(Description__contains=search_text))

            settingsTotalCount = queryset.count()
            paginator = Paginator(queryset, request.GET.get('pageSize'))
            try:
                queryset = paginator.page(request.GET.get('pageNumber'))
            except PageNotAnInteger:
                queryset = paginator.page(1)
            except EmptyPage:
                queryset = paginator.page(paginator.num_pages)

            serializer = SettingSerializer(queryset, many=True)

            #isItOk = serializer.is_valid()
            #headersJson = {'headers': ','.join(request.headers)}
            response = Response({'settings': serializer.data})
            response['X-Total-Count'] = settingsTotalCount
            return response
        else:
            return Response('no_permissions', status=status.HTTP_403_FORBIDDEN)

    #def post(self, request, *args, **kwargs):
    #    '''
    #    Create the setting with given todo data
    #    '''
    #    data = {
    #        'task': request.data.get('task'), 
    #        'completed': request.data.get('completed'), 
    #        'user': request.user.id
    #    }
    #    serializer = TodoSerializer(data=data)
    #    if serializer.is_valid():
    #        serializer.save()
    #        return Response(serializer.data, status=status.HTTP_201_CREATED)

    #    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)