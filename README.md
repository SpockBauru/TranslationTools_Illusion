# TranslationToolsHS2
  Translations Tools for Honey Select 2

## ReleaseToolHS2

  Read the translations repository folder and make a file clean to release. The release file will have:

  1) Cleaned all empty and commented lines in RedirectedResources (if it exist). If a file only has empty or commented lines it will be ignored.
  2) Folders RedirectedResources, Text and Texture will be zipped (if they exist).
  3) readme, license and config folder.

  HOW TO USE

  Drag and drop the GitHub project folder on "ReleaseToolHS2.exe".

  **IMPORTANT:** The file `config\ AutoTranslatorConfig.ini` must have the Language key configured correctly.

  **v2 - [Download](https://github.com/SpockBauru/TranslationToolsHS2/releases/tag/r5)**

## SplitMTL

  SplitMTL - Split Machine Translations

  This tool is intended to be used in translation projects of games using XUnity Auto Translator with the plugin TextResourceRedirector. It read Machine Translations and split into the folder "RedirectedResources", which is less resource intensive, decrease game loading and uses less RAM (but uses more disk space).

  The folder "RedirectedResources" must have a dump from the game text in order to work.

  HOW IT WORKS
  1) This tool reads all Translated text from the source folder (asked when you open) and from the destination folder (MachineTranslation).
  2) Then it reads all Untranslated text from the files "translation.txt" in the source folder and write the translated version in files called "zz_machineTranslation.txt" in "MachineTranslation", but imitating the structure from the source folder.
  3) Each file "zz_machineTranslation.txt" will contain in its header a copy of the file "Header.txt", so it can be used for other languages than English.

  After that you can inspect the files "zz_machineTranslation.txt" and copy to the source folder.

**v1 - [Download](https://github.com/SpockBauru/TranslationToolsHS2/releases/tag/r6)**

## StyleCheck

  This tool is intended to be used with translation files that are used by XUnity Auto Translator.

  It's a simple tool that substitutes strings for the translated part using rules from "Substitutions.txt". The original untranslated part (before the "=" sign) is not affected.

  It can also use Regular Expressions (Regex).

  It's useful to make simple style checks, such as using uppercase letters after a dot. But with Regexes and some imagination it can do more powerful substitutions.

  HOW TO USE
  1) Make the desired rules in "Substitutions.txt".
     All rules will be implemented following the order in this file, so order matters.
  2) Drag and drop the desired file in "StyleCheck.exe"

  **v1 - [Download](https://github.com/SpockBauru/TranslationToolsHS2/releases/tag/r7)**

## MachineTranslate

  This simple tool reads the untranslated text from a given folder, translate them with online machine services, perform style checks and make a file with all translations.

  HOW TO USE

  Open the file "MachineTranslate.exe" and enter the desired folder containing untranslated text. The result will be in the file "MachineTranslation\MachineTranslationsFinal.txt".

  There are more files in this folder, they are used to resume the translation process if the tool was closed for some reason.

  CONFIGURATION FILES
  1) Languages.txt: Set the original and destination languages using the GoogleTranslate Languages Codes in the format "Original:Destination". Example: ja:en
  2) Retranslate.txt: All machine translation sites are prone to errors. This file set the rules to detect severe mistranslations that are unrecoverable, like when a word is not translated at all. It uses Regular Expressions (REGEX). All lines that match these rules will be retranslated with BingTranslator.
  3) Substitutions.txt: Substitute common errors with the format: oldWord=newWord
     Regular Expressions can be used with the format: r:"regex expression"="substitution" (quotes needed)

  HOW IT WORKS
  1) Reads all untranslated lines in .txt files from the given source folder and its subfolders.
     Are considered as untranslated lines starting with "//" and having an "=" sign. Example: //寝起き=
  2) Reads the translated lines from both the source folder and the destination folder "MachineTranslation".
     Are considered as translated lines without "//" in the beginning and with some text after the "=" sign. Example: 寝起き=Wake up
  3) Translate all untranslated lines that don't have a translation in both source and destination folder using GoogleTranslate.
     Translated lines are saved in "MachineTranslation\1-GoogleTranslateRAW.txt".
  4) Detects errored translations using Retranslate.txt and retranslate using BingTranslator
     Translated lines are saved in "MachineTranslation\2-BingTranslateRAW.txt".
  5) Substitutes all strings using Substitutions.txt. (useful for style check)
  6) The final file is saved as "MachineTranslation\MachineTranslationsFinal.txt".

**v2 - [Download](https://github.com/SpockBauru/TranslationToolsHS2/releases/tag/r8)**

## Translate Duplicates

  Searches an entire folder if an untranslated line has been translated in other place and writes that translation. If there are several translations for the same sentence, only the first one is used. Useful for RedirectedResources folder. Use with caution!

  **v1 - [Download](https://github.com/SpockBauru/TranslationToolsHS2/releases/tag/r2)**

## Delete Duplicates

  Create a new file without duplicated lines. Its a way faster than Notepad++

  **v1 - [Download](https://github.com/SpockBauru/TranslationToolsHS2/releases/tag/r1)**
