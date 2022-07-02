# !/bin/bash
python manage.py migrate
fixtures=$(ls NewsServiceApp/migrations/seed/)
while IFS= read -r fixture; do
    echo -n "Seeding "
    echo $fixture
    python manage.py loaddata NewsServiceApp/migrations/seed/$fixture
done <<< "$fixtures"