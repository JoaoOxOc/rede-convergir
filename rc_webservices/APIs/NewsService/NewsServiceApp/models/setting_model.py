from django.db import models
from django.db.models.fields import NullBooleanField

'''
how to make migration: python manage.py makemigrations NewsServiceApp
how to seed: python manage.py loaddata ./NewsServiceApp/migrations/seed/settings.json
'''
class Setting(models.Model):
    Key = models.CharField(max_length=30)
    Value = models.CharField(max_length=300)
    DefaultValue = models.CharField(max_length=300)
    Description = models.CharField(max_length=100)
    SettingType = models.IntegerField()
    CreatedAt = models.DateTimeField()
    UpdatedAt = models.DateTimeField(null=True)
    UpdatedBy = models.CharField(max_length=100, null=True)
