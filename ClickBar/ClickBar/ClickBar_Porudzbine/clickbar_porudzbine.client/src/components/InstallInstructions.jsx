import React from 'react';

const InstallInstructions = () => {
    return (
        <div>
            <h2>Instalacija Cordova Aplikacije</h2>
            <p>Da biste instalirali Cordova aplikaciju, pratite sledeće korake:</p>
            <ol>
                <li>Kliknite na <a href="http://192.168.0.150:5000/download/app-debug.apk" download>ovaj link</a> za preuzimanje APK fajla.</li>
                <li>Nakon preuzimanja, otvorite APK fajl na vašem uređaju.</li>
                <li>Možda ćete morati da omogućite instalaciju aplikacija iz nepoznatih izvora u podešavanjima uređaja.</li>
                <li>Pratite uputstva na ekranu za instalaciju aplikacije.</li>
            </ol>
        </div>
    );
};

export default InstallInstructions;