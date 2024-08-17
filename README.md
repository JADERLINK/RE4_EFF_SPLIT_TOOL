# RE4_EFF_SPLIT_TOOL

Extract and repack RE4 EFF files (RE4 2007/UHD/PS2)

**Translate from Portuguese Brazil**

Tool destinada a dividir o arquivo EFF em 3 partes, você deve usar outra tool para editar o arquivo .EFFBLOB resultante da extração.

**Update V.1.0.1**
<br>Nessa nova versão, para arquivos "0000.EFF", irá gerar uma pasta de nome "0000_EFF", mudança feita para evitar sobreposição de arquivos.

## Extract

Use o arquivo .bat para extrair: 
<br>(nota: a versão de PS2 também serve para o Re4 2007)
<br> RE4_PS2_EFF_SPLIT EXTRACT.bat
<br> RE4_UHD_EFF_SPLIT EXTRACT.bat
<br>
<br> Para o exemplo "core_001.EFF" será gerados os arquivos:
<br> * "core_001.IDX_UHD_EFF_SPLIT" = arquivo usado para o repack;
<br> * "core_001/Effect Models/" = pasta contendo os arquivos BIN/TPL (Table10);
<br> * "core_001/Effect TPL/" = pasta contendo os arquivos TPL (table05);
<br> * "core_001/core_001.EFFBLOB" = arquivo que contem as outras tabelas, você deve usar outra tool para poder editar esse arquivo.

## Repack

Use o arquivo .bat para recompactar:
<br> RE4_PS2_EFF_SPLIT REPACK.bat
<br> RE4_UHD_EFF_SPLIT REPACK.bat
<br>
<br> Tendo como exemplo o arquivo anterior, será necessários os arquivos para o repack:
<br> * "core_001.IDX_UHD_EFF_SPLIT"
<br> * "core_001/Effect Models/" = a quantidade de arquivo colocado no repack, é enumerada em ordem sem pular numeração;
<br> * "core_001/Effect TPL/" = a quantidade de arquivo colocada no repack, é enumerada em ordem sem pular a numeração;
<br> * "core_001/core_001.EFFBLOB"

**At.te: JADERLINK**
<br>2024-08-17