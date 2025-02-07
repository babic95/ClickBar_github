import { useState, useEffect, useRef } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import TextField from '@mui/material/TextField';
import { styled } from '@mui/material/styles';
import List from '@mui/material/List';
import './Stolovi.css'
import { ScrollMenu } from 'react-horizontal-scrolling-menu';
import 'react-horizontal-scrolling-menu/dist/styles.css';
import Button from '@mui/material/Button';
import { DraggableFix } from '../../components/DraggableFix';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const StoloviPrebacivanjeRadnika = () => {
    const navigate = useNavigate()
    let location = useLocation();
    const ref = useRef(null);
    const [left, setLeft] = useState(0);
    const [top, setTop] = useState(0);
    const [width, setWidth] = useState(0);


    const [porudzbinaOriginal, setPorudzbinaOriginal] = useState(location.state.porudzbina);
    const [porudzbina, setPorudzbina] = useState(location.state.porudzbina);
    const [deloviSale, setDeloviSale] = useState([]);
    const [currentDeoSaleId, setCurrentDeoSaleId] = useState(null);
    const [stolovi, setStolovi] = useState([]);

    const [currentSto, setCurrentSto] = useState(null);
    const [open, setOpen] = useState(false);

    const [users, setUsers] = useState([
        {
            'Id': '-1',
            'Name': 'Izaberite konobara'
        }])
    const [userForChange, setUserForChange] = useState('');

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchData();
        const { offsetWidth, offsetLeft, offsetTop } = ref.current;
        setWidth(offsetWidth);
        setLeft(offsetLeft);
        setTop(offsetTop);

    }, []);

    useEffect(() => {

        function watchScroll() {
            window.addEventListener("scroll", logit);
        }
        watchScroll();
        return () => {
            window.removeEventListener("scroll", logit);
        };
    });

    const logit = () => {
        if (currentDeoSaleId > 0) {
            const s = []

            var deoSale = deloviSale.find(g => g.id === currentDeoSaleId);

            if (deoSale !== null) {
                deoSale.stolovi.map((sto) => {
                    sto.left = left + (width * (sto.sto.x / 100));
                    sto.top = top + (width * (sto.sto.y / 100)) + 60 - window.pageYOffset;

                    sto.w = width * (sto.sto.width / 100);
                    sto.h = width * (sto.sto.height / 100);

                    s.push(sto)
                });

                setStolovi(s);
            }
        }
    }

    const fetchData = async () => {
        try {
            setLoading(true);
            const response = await fetch('api/sto/stoPorudzbine'); // Replace with your API endpoint
            if (!response.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await response.json();

            if (data !== null) {

                const n = []

                data.map((deoSale) => {
                    n.push(deoSale);
                })

                setDeloviSale(n);
            }

            setLoading(false);
        } catch (error) {
            setError("Desila se greška: " + error.message);
            setLoading(false);
        }
    };

    const handleSubmitDeoSale = (id) => {

        const s = []

        var deoSale = deloviSale.find(g => g.id === id);

        if (deoSale !== null) {
            deoSale.stolovi.map((sto) => {
                sto.left = left + (width * (sto.sto.x / 100));
                sto.top = top + (width * (sto.sto.y / 100)) + 60;

                sto.w = width * (sto.sto.width / 100);
                sto.h = width * (sto.sto.height / 100);

                sto.isOpen = false;

                sto.items.map(item => {
                    item.nameFront = item.naziv + " - " + item.kolicina + item.jm;

                    item.zeljeFront = [];
                    if (item.zelje !== null &&
                        item.zelje !== "") {
                        const splitItems = item.zelje.split(", ");

                        splitItems.map(z => {
                            item.zeljeFront.push(z);
                        })
                    }
                })

                s.push(sto)
            })
        }
        setCurrentDeoSaleId(id);
        setStolovi(s);
    }

    const handleClose = () => {
        setOpen(false);
    }

    const handleSubmitSto = async (sto) => {
        setOpen(true);

        const stoName = "S" + sto.sto.name

        console.log(stoName);

        setCurrentSto(stoName);

        try {
            const responseUsr = await fetch('api/user/allUsers'); // Replace with your API endpoint
            if (!responseUsr.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await responseUsr.json();

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
    }

    const clickOnBack = () => {

        navigate('/sto', { state: { porudzbina: porudzbinaOriginal } });
    }

    const clickOnChangeUser = async () => {
        if (userForChange !== null &&
            userForChange !== "" &&
            currentSto !== null) {

            try {
                const moveKonobar = {
                    fromKonobarId: porudzbina.user,
                    toKonobarId: userForChange,
                    stoId: currentSto
                }

                const requestOptionsMK = {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(moveKonobar)
                };

                const responseMK = await fetch('api/sto/moveWorker', requestOptionsMK); // Replace with your API endpoint
                if (!responseMK.ok) {

                    toast.error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži', { autoClose: 2000 });
                    throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
                }

                toast.success('Uspešno ste prebacili porudzbine sa stola na drugog konobara!', { autoClose: 1000 });
                await timeout(1500);
            }
            catch (error) {
                console.log(error)
                toast.error('Desila se greška, proverite konekciju sa internetom.', { autoClose: 2000 });
            }
        }
    }

    const handleUserChange = (event) => {
        const { name, value } = event.target;

        setUserForChange(value);
    }

    const handleCloseChangeUser = () => {
        setOpen(false);
    }

    function timeout(delay) {
        return new Promise(res => setTimeout(res, delay));
    }

    const FireNav = styled(List)({
        '& .MuiListItemButton-root': {
            paddingLeft: 24,
            paddingRight: 24,
        },
        '& .MuiListItemIcon-root': {
            minWidth: 0,
            marginRight: 16,
        },
        '& .MuiSvgIcon-root': {
            fontSize: 20,
        },
    });

    return (
        <div className="App213">
            <div className="appForm123">
                <Dialog
                    open={open}
                    onClose={handleCloseChangeUser}
                    PaperProps={{
                        component: 'form',
                        onSubmit: (event) => {
                            event.preventDefault();
                            const formData = new FormData(event.currentTarget);
                            const formJson = Object.fromEntries(formData.entries());
                            const email = formJson.email;
                            console.log(email);
                            handleCloseChangeUser();
                        },
                    }}
                >
                    <DialogTitle>Prebaci porudzbine sa stola {currentSto === null ? null : currentSto}</DialogTitle>
                    <DialogContent>
                        <div className="dugmici">
                            <List
                                sx={{ width: '100%' }}
                                component="nav"
                                aria-labelledby="nested-list-subheader"
                            >
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
                                            value={userForChange}
                                            inputProps={{
                                                style: {
                                                    fontSize: "0.9em",
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
                            </List>
                        </div>
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleCloseChangeUser}>Zatvori</Button>
                        <Button type="submit" onClick={clickOnChangeUser}>Predaj smenu</Button>
                    </DialogActions>
                </Dialog>

                <div className="centerB">
                    <p className="formTitleLink">Prebacivanje porudzbine sa stola na drugog radnika</p>
                </div>
                <ArrowBackIcon
                    onClick={clickOnBack}
                    className="fa-plus-circle"
                    sx={{ fontSize: 40 }} />

                <div className="konobarDiv">
                    <p className="konobarP konobarDiv">Konobar: {porudzbinaOriginal.userName}</p>
                </div>

                <div className="dugmici">
                    <ScrollMenu>
                        {deloviSale.map((deoSale) => (
                            <div key={deoSale.id}
                                className="rows">
                                <button className="formFieldButtonGrupa"
                                    onClick={() => handleSubmitDeoSale(deoSale.id)}>{deoSale.name}</button>
                            </div>
                        ))}
                    </ScrollMenu >
                </div>

                <div ref={ref} id="sala" className="pozicijaStolova" style={{
                    height: width
                }}>
                    {stolovi.map((sto) => (
                        <DraggableFix key={sto.sto.id}
                            style={{
                                left: sto.left,
                                top: sto.top
                            }}
                        >
                            <div onClick={() => handleSubmitSto(sto)}
                                style={{
                                    backgroundColor: sto.sto.color, //'#EC8B5E',
                                    height: sto.h,
                                    width: sto.w
                                }}>{sto.sto.name}</div>
                        </DraggableFix>
                    ))}
                </div>

                <div className="dugmici">
                    <p className="formTitleLink "> Stolovi: </p>
                    <List
                        sx={{ width: '100%' }}
                        component="nav"
                        aria-labelledby="nested-list-subheader"
                    >
                        {stolovi.map((sto) => (
                            <button key={sto.sto.id}
                                className="formFieldButtonNadgrupa"
                                style={{
                                    backgroundColor: sto.sto.color
                                }}
                                onClick={() => handleSubmitSto(sto)}>{sto.sto.name}</button>
                        ))}
                    </List>
                </div>
            </div>

            <ToastContainer />
        </div>
    );
}
export default StoloviPrebacivanjeRadnika;