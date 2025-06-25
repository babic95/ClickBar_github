import { useState, useEffect } from "react";
import { useNavigate, Link } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import './Login.css'
import imageLogo from '../../icons/logo.png';
import PrintComponent from '../../components/PrintComponent';

const Login = () => {
    const [deferredPrompt, setDeferredPrompt] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [users, setUsers] = useState([
        {
            'Id': '-1',
            'Name': 'Izaberite konobara'
        }])
    const [porudzbina, setPorudzbina] = useState({
        'user': '',
        'artikli': []
    })
    const navigate = useNavigate()

    useEffect(() => {
        fetchData();

        window.addEventListener('beforeinstallprompt', (e) => {
            e.preventDefault();
            setDeferredPrompt(e);
        });
    }, []);

    const fetchData = async () => {
        try {
            const response = await fetch('/api/user/allUsers'); // Replace with your API endpoint
            if (!response.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await response.json();

            const radnici = [
                {
                    'Id': '-1',
                    'Name': 'Izaberite konobara'
                }];

            if (data !== null) {
                data.map((radnik) => {
                    var r = {
                        'Id': radnik.id,
                        'Name': radnik.name
                    }
                    radnici.push(r)
                })
            }
            setUsers(radnici);
            setLoading(false);
        } catch (error) {
            setError("Desila se greška: " + error.message);
            setLoading(false);
        }
    };

    const handleUserChange = (event) => {
        const { name, value } = event.target;

        setPorudzbina((prevState) => ({
            ...prevState,
            [name]: value
        }));
    }

    const handleSubmit = (event) => {

        if (porudzbina.user === null ||
            porudzbina.user === '-1') {
            console.log("Greska prilikom logovanja");

            setError("Morate izabrati konobara!");
            setLoading(false);

            return
        }

        event.preventDefault();

        const user = porudzbina.user
        console.log("Ulogovan je user: " + user);

        const u = users.find(uu => uu.Id == porudzbina.user);

        porudzbina.userName = u.Name;

        navigate('/sto', { state: { porudzbina: porudzbina } });
    }

    const handleInstallClick = () => {
        if (deferredPrompt) {
            deferredPrompt.prompt();
            deferredPrompt.userChoice.then((choiceResult) => {
                if (choiceResult.outcome === 'accepted') {
                    console.log('User accepted the A2HS prompt');
                } else {
                    console.log('User dismissed the A2HS prompt');
                }
                setDeferredPrompt(null);
            });
        }
    };

    return (
        <div className="App1">
            <div className="appAside" />
            <div className="appForm">


                <div className="formTitle">
                    <p className="formTitleLink"> CCS Prudžbine </p>


                </div>

                <div className="formCenter">
                    <form className="formFields" onSubmit={handleSubmit}>

                        <div className="formField">
                            {loading ? (
                                <p>Sačekajte...</p>
                            ) : error ? (
                                <p>Greška: {error}</p>
                            ) : (
                                <TextField fullWidth
                                    type="search"
                                    InputLabelProps={{
                                        sx: {
                                            fontSize: "0.9em",
                                            fontWeight: 1000,
                                            color: "white",
                                            border: "2px solid black"
                                        }
                                    }}
                                    name="user"
                                    onChange={(e) => handleUserChange(e)}
                                    required
                                    select
                                    SelectProps={{
                                        native: true,
                                    }}
                                    value={porudzbina.user}
                                    inputProps={{
                                        style: {
                                            fontSize: "0.9em",
                                            color: "white",
                                        }
                                    }}
                                >
                                    {users.map((option, index) => (
                                        <option
                                            key={index}
                                            value={option.Id}
                                        >
                                            {option.Name}
                                        </option>
                                    ))}
                                </TextField>
                            )}
                        </div>

                        <div className="centerB">
                            <button className="formFieldButton">Prijavi se</button>{" "}
                        </div>

                        <div className="centerB logo">
                            <img height="150px" src={imageLogo} />
                        </div>
                        <div className="centerB">
                            <label className="formFieldLabel" htmlFor="email">
                                CleanCodeSirmium
                            </label>
                        </div>
                        <div className="centerB">
                            <label className="formFieldLabel" htmlFor="email">
                                tel: +381/64-44-20-296
                            </label>
                        </div>
                        <div className="centerB">
                            <label className="formFieldLabel" htmlFor="email">
                                email: cleancodesirmium@gmail.com
                            </label>
                        </div>
                    </form>
                </div>
                {deferredPrompt && (
                    <div className="pwa-prompt">
                        <p>Would you like to install this app?</p>
                        <button onClick={handleInstallClick}>Install</button>
                    </div>
                )}
            </div>
        </div>
    );
}
export default Login;