# Somtoday2MicrosoftSchoolDataSync
Open source oplossing om Microsoft Teams te kunnen gebruiken met School Data Sync met gegevens uit [Somtoday](https://www.som.today/). 
Create Microsoft School Data Sync CSV-files using the Somtoday webservices. 

De CSV bestanden zijn nodig, omdat een directe verbinding niet mogelijk is. Somtoday biedt **geen** ondersteuning voor de OneRoster standaard: https://www.imsglobal.org/oneroster-v11-final-specification. Zodra zij dat wel doen, is deze applicatie overbodig.

![Logo](/Somtoday2MicrosoftSchoolDataSync/Resources/SOMSDS.ico)

## Functionaliteiten

* Gebruikt de Windows Event Viewer om de status te loggen.
* De lesgroepen van het huidige schooljaar worden opgevraagd.
* Lesgroepen zonder docent worden **niet** verwerkt.
* Lesgroepen zonder leerling worden **niet** verwerkt.
* Lesgroepen krijgen een "Uniek ID" op basis van de vestigingsafkorting. Dit ziet men **niet** terug in de DisplayName van de lesgroep.
* Ongeldige tekens worden vervangen. https://support.microsoft.com/en-us/kb/905231


## Installatie
[Download het ZIP-bestand](Somtoday2MicrosoftSchoolDataSync.zip) en pak de bestanden uit.

Tip: Maak een scheduled task aan. Het synchroniseren van leerlingegevens is alleen ’s nachts toegestaan vanuit Somtoday.


## Configuratiestappen na installatie
Ga naar de installatie directory en bewerk Somtoday2MicrosoftSchoolDataSync.exe.config in een text editor.

### endpoint address
Sinds 13 februari 2021 is het adres voor iedereen: https://oop.somtoday.nl/services/umService

### BooleanFilterBylocation

Filter toepassen of alle vestigingen opvragen.
* False: alle vestigingen opvragen.
* True: alleen onderstaande vestigingen opvragen.

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
Maak voor elke vestiging een eigen map aan. Dit kan gebruikt worden als je meerdere synchronisatieprofielen hebt binnen School Data Sync.
* False: alle gegevens in bovenstaande OutputDirectory opslaan.
* True: maak voor elke vestiging een eigen directory aan. Dit worden subdirectories in de OutputDirectory.

Dit is noodzakelijk als je meer dan 500.000 rijen in één bestand hebt. https://docs.microsoft.com/en-us/schooldatasync/csv-files-for-school-data-sync#required-csv-files-for-school-data-sync



## Volgende stappen

Synchroniseer de CSV-bestanden m.b.v. de SDS Flow Connector.
https://docs.microsoft.com/en-us/schooldatasync/csv-file-sync-automation



# Koppelen met Magister
Gebruikt jouw school Magister en zoek je een koppeling tussen Magister en School Data Sync? Bezoek dan https://github.com/sikkepitje/TeamSync
