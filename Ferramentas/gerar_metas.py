# -*- coding: utf-8 -*-
"""
Cria os arquivos .meta que faltam dentro de Assets/ (pastas, scripts, asmdef,
textos e cenas), com GUID determinístico: md5("cincoestrelas:" + caminho).

Assim os GUIDs não mudam entre máquinas e as cenas (que apontam para os
scripts pelo GUID) continuam funcionando depois de clonar o repositório.
Os .meta que já existem não são alterados (as imagens têm o .meta gerado
por gerar_arte.py e Assets/Settings veio do template do Unity).

Uso (na raiz do projeto):  py Ferramentas/gerar_metas.py
"""
import hashlib
import io
import os

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS = os.path.join(RAIZ, "Assets")


def guid_de(caminho_relativo):
    return hashlib.md5(("cincoestrelas:" + caminho_relativo).encode("utf-8")).hexdigest()


def meta_pasta(guid):
    return ("fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
            "  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % guid)


def meta_script(guid):
    return ("fileFormatVersion: 2\nguid: %s\nMonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n"
            "  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n" % guid)


def meta_asmdef(guid):
    return ("fileFormatVersion: 2\nguid: %s\nAssemblyDefinitionImporter:\n  externalObjects: {}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n" % guid)


def meta_texto(guid):
    return ("fileFormatVersion: 2\nguid: %s\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n" % guid)


def meta_padrao(guid):
    return ("fileFormatVersion: 2\nguid: %s\nDefaultImporter:\n  externalObjects: {}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n" % guid)


def meta_para(caminho_abs, rel):
    guid = guid_de(rel)
    if os.path.isdir(caminho_abs):
        return meta_pasta(guid)
    ext = os.path.splitext(caminho_abs)[1].lower()
    if ext == ".cs":
        return meta_script(guid)
    if ext == ".asmdef":
        return meta_asmdef(guid)
    if ext in (".txt", ".json", ".md", ".csv", ".xml", ".bytes"):
        return meta_texto(guid)
    return meta_padrao(guid)


def caminhos(pasta):
    for raiz, pastas, arquivos in os.walk(pasta):
        pastas[:] = [p for p in pastas if not p.startswith(".")]
        for p in pastas:
            yield os.path.join(raiz, p)
        for a in arquivos:
            if a.endswith(".meta") or a.startswith("."):
                continue
            yield os.path.join(raiz, a)


def main():
    criados = 0
    for c in caminhos(ASSETS):
        meta = c + ".meta"
        if os.path.exists(meta):
            continue
        rel = os.path.relpath(c, RAIZ).replace("\\", "/")
        with io.open(meta, "w", encoding="utf-8", newline="\n") as f:
            f.write(meta_para(c, rel))
        criados += 1
        print("  .meta criado: " + rel)
    print("Pronto: %d .meta criados." % criados)


if __name__ == "__main__":
    main()
