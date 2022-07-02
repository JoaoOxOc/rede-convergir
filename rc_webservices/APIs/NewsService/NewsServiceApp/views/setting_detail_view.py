from rest_framework import viewsets, status, permissions
from rest_framework.decorators import api_view
from rest_framework.views import APIView
from rest_framework.response import Response
from NewsServiceApp.ViewModels.serializers.setting_serializer import SettingSerializer
from NewsServiceApp.models.setting_model import Setting
import json
from datetime import datetime

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
"""
class SettingDetailApiView(APIView):

    def get_object(self, setting_id):
        '''
        Helper method to get the object with given setting_id
        '''
        try:
            return Setting.objects.get(id=setting_id)
        except Setting.DoesNotExist:
            return None

    def get(self, request, setting_id, *args, **kwargs):
        '''
        Retrieves the Setting with given setting_id
        '''
        setting_instance = self.get_object(setting_id)
        if not setting_instance:
            return Response(
                {"message": "Object with setting id " + str(setting_id) + " does not exists"},
                status=status.HTTP_404_NOT_FOUND
            )

        serializer = SettingSerializer(setting_instance)
        return Response(serializer.data, status=status.HTTP_200_OK)

    def put(self, request, setting_id, *args, **kwargs):
        '''
        Updates the setting with given setting_id if exists
        '''
        setting_instance = self.get_object(setting_id)
        if not setting_instance:
            return Response(
                {"res": "Object with setting id " + str(setting_id) + " does not exists"}, 
                status=status.HTTP_404_NOT_FOUND
            )
        data = {
            'Description': request.data.get('description'), 
            'Value': request.data.get('value'), 
            'UpdatedAt': datetime.now(),
            'UpdatedBy': request.headers.get('UserId')
        }
        serializer = SettingSerializer(instance = setting_instance, data=data, partial = True)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_200_OK)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
