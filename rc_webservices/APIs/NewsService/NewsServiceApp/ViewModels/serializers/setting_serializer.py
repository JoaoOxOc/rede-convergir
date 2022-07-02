from rest_framework import serializers
from NewsServiceApp.models.setting_model import Setting


class SettingSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Setting
        fields = ['id', 'Key', 'Value', 'DefaultValue', 'Description', 'SettingType', 'CreatedAt', 'UpdatedAt', 'UpdatedBy']
