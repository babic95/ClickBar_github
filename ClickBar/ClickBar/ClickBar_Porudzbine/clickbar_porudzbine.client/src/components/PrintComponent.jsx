import React, { useEffect } from 'react';

const PrintComponent = () => {
    const handlePrint = () => {
        if (window.cordova && window.BluetoothPrinter) {
            window.BluetoothPrinter.listDevices(
                (devices) => {
                    if (devices.length > 0) {
                        const printer = devices[0]; // Pretpostavimo da je prvi uređaj na listi naš štampač
                        window.BluetoothPrinter.connect(
                            printer.address,
                            () => {
                                const text = "Hello, POS Printer!";
                                window.BluetoothPrinter.printText(
                                    text,
                                    () => {
                                        alert("Štampanje uspešno");
                                    },
                                    (error) => {
                                        alert("Greška pri štampanju: " + error);
                                    }
                                );
                            },
                            (error) => {
                                alert("Greška pri povezivanju: " + error);
                            }
                        );
                    } else {
                        alert("Nema dostupnih Bluetooth uređaja");
                    }
                },
                (error) => {
                    alert("Greška pri listanju uređaja: " + error);
                }
            );
        } else {
            alert("Cordova nije dostupna ili plugin nije učitan");
        }
    };

    useEffect(() => {
        document.addEventListener('deviceready', () => {
            console.log('Cordova je spremna');
        }, false);
    }, []);

    return (
        <div>
            <button onClick={handlePrint}>Štampaj</button>
        </div>
    );
};

export default PrintComponent;