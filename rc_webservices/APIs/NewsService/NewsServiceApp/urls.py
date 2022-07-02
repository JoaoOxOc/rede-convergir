from django.conf.urls import url
from django.urls import path, include
from .views import (
    SettingsListApiView,
    SettingDetailApiView
)

urlpatterns = [
    path('settings/', SettingsListApiView.as_view()),
    path('setting/<int:setting_id>/', SettingDetailApiView.as_view()),
]
