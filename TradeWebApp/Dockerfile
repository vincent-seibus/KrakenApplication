# Notre image se base sur celle d'aspnet
FROM microsoft/aspnet:1.0.0-rc1-final-coreclr
 
# Copie des sources dans un répertoire app
COPY . /app
 
# Curseur dans le répertoire app
WORKDIR /app
 
# Restauration des dépendances
RUN ["dnu", "restore"]
 
# Notre service est exposé sur le port 5000
EXPOSE 5000
 
# Commande de lancement du service à exécuter au lancement du conteneur
ENTRYPOINT ["dnx","-p","project.json", "web"]