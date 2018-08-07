# Somtoday2MicrosoftSchoolDataSync
Open source oplossing om Microsoft Teams te kunnen gebruiken met School Data Sync met gegevens uit Somtoday. 

Create Microsoft School Data Sync CSV-files using the Somtoday webservices. 

[Download de installatie](setup.exe)

Tip: Maak een scheduled task aan. Het synchroniseren van leerlingegevens is alleen ’s nachts toegestaan vanuit Somtoday.



## Configuratiestappen
* Bewerk Somtoday2MicrosoftSchoolDataSync.exe.config
Wijzig het endpoint address naar de URL van de Somtoday UmService van jouw school.

### BooleanFilterBylocation

Filter toepassen of alle vestigingen opvragen.
* waarde: False | alle vestigingen opvragen
* waarde: True | alleen onderstaande vestigingen opvragen

### IncludedLocationCode

Indien BooleanFilterBylocation op True staat, kan je hier de afkortingen van de vestigingen opgeven. Puntkomma gescheiden.
* Voorbeeld: AB;cd;Ef


### umServiceBrinNr

Het BRINnummer van de hoofdvestiging
* Voorbeeld: 01AB


### umServiceUsername

De username van het UmService-account (TIP: maak hiervoor een apart account aan)


### umServicePassword

Het wachtwoord van de UmService-account


### OutputDirectory

De CSV bestanden worden opgeslagen in deze directory.
* Voorbeeld: C:\SomSync\CSV


### SeperateOutputDirectoryForEachLocation

Maak voor elke vestiging een eigen directory aan.




Synchroniseer de CSV-bestanden m.b.v. de School Data Sync Toolkit.
https://docs.microsoft.com/en-us/schooldatasync/install-the-school-data-sync-toolkit
