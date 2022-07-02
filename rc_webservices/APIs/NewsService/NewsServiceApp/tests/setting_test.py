from django.test import TestCase, Client
from rest_framework.test import APIRequestFactory
from django.urls import reverse

class TestSettingCrud(TestCase):
    """
    This class contains tests for settings CRUD
    """
    def setUp(self):
        """
        This method runs before the execution of each test case.
        """
        self.client = Client()
        self.url = reverse("NewsServiceApp:setting")

    def test_get_settings(self):
        factory = APIRequestFactory()
        request = factory.get('/settings/')
