151129
no servidor e na fcul
  update nodejs to 0.10.40 (no servidor já está)
  reinstall all the modules
  mudar directoria do codigo: github/rede-conevrgir-3 (para ficar como no portatil)
  actualizar ficheiro configuracao nginx

adicionar analytics
indicar campos obrigatorios
mudar repo para rede-convergir + melhorar estrutura das directories
on each list refresh, the selected initiative should be deselected (?)
selectTimestamp - avoid show the loading icon if we just selected the initiative 
run the sql scripts (have been updated)




1) execute shp2pgsql
shp2pgsql -D -I -s 4326                 "/path/to/shape_file.shp" geo.shape_file | psql --dbname=150608

NOTE: the table name (in the "geo" schema) should be in lower case


2) create row in the shapes table

select * from shapes_create('{"srid":4326,"description":{"en":""},"schema_name":"geo","table_name":"shape_file","file_id":2145,"owner_id":1}')


3) style + legend + interactivity


4) export to mbtiles: 8-16 (or only 8-9 to begin)


5) copy-paste the .mml file from the project 

5a) re-use the following:

    bounds
    center
    minzoom
    maxzoom
    name
    description
    legend

5b) add these:
    id (same as the file name)
    isCirac: true,
    tiles (same as the file name)
    grids (same as the file name)
    template (copy from template_teaser.template_teaser)


    

    

--

shp2pgsql -D -I -s 4326      "/home/pvieira/cirac/SERVIDOR/CIRAC/outputs/mapas/vulnerabilidade/Indice-Vulnerabilidade/Map-SHP/BGRI/Indice-de-vulnerabilidade/cirac_vul_bgri_FVI_N.shp" geo.cirac_vul_bgri_fvi_n | psql --dbname=150608

select * from shapes_create('{"srid":4326,"description":{"en":""},"schema_name":"geo","table_name":"cirac_vul_bgri_fvi_n","file_id":1,"owner_id":1}')

--


shp2pgsql -D -I -s 4326  \
"/home/pvieira/cirac/SERVIDOR/CIRAC/outputs/mapas/vulnerabilidade/Indice-Vulnerabilidade/Map-SHP/BGRI/Indice-de-vulnerabilidade/Percentile-75/cirac_vul_bgri_FVI_75.shp" geo.cirac_vul_bgri_fvi_75 | psql --dbname=150608

select * from shapes_create('{"srid":4326,"description":{"en":""},"schema_name":"geo","table_name":"cirac_vul_bgri_fvi_75","file_id":1,"owner_id":1}')

--





Quando se selecciona um layer do cirac, já nao é possível voltar a nao ver esse layer


interactions:
    -(list -> map) in the list of initiatives, when the user clicks the "mapa" button, the map should center in that point
    -(map -> list) when the user clicks one icon in the map, the list should scroll to place that initiative in the top
    -(map -> list) when the user drags or zooms the map, the collection of visible  icons will change; the list should always reflect what the user is currently seeing (and the scroll position should be maitained when possible)
    -(filters -> map/list) when the user updates de filters, the list and map should update
    -(search box -> map/list) 