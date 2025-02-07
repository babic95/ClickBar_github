import { useState, useEffect, useRef } from "react";
import { useLocation } from "react-router-dom";
import TextField from '@mui/material/TextField';
import { createTheme } from '@mui/material/styles';
import './Admin.css'
import { ScrollMenu } from 'react-horizontal-scrolling-menu';
import 'react-horizontal-scrolling-menu/dist/styles.css';
import Button from '@mui/material/Button';
import { Draggable } from '../../components/Draggable';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';

const Admin = () => {
    let location = useLocation();
    const ref = useRef(null);
    const [left, setLeft] = useState(0);
    const [top, setTop] = useState(0);
    const [width, setWidth] = useState(0);

    const elementSala = document.querySelector("div#sala");

    const [currentSto, setCurrentSto] = useState(null);
    const [widthSto, setWidthSto] = useState(0);
    const [heightSto, setHeightSto] = useState(0);
    const [yScroll, setYScroll] = useState(0);

    const [response, setResponse] = useState([]);
    const [deloviSale, setDeloviSale] = useState([]);
    const [currentDeoSaleId, setCurrentDeoSaleId] = useState(null);
    const [currentDeoSaleName, setCurrentDeoSaleName] = useState(null);
    const [stolovi, setStolovi] = useState([]);

    const [newDeoSale, setNewDeoSale] = useState({
        name: ''
    });

    const [open, setOpen] = useState(false);

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
                    sto.left = left + (width * (sto.x / 100));
                    sto.top = top + (width * (sto.y / 100)) + 60 - window.pageYOffset;

                    sto.w = width * (sto.width / 100);
                    sto.h = width * (sto.height / 100);

                    s.push(sto)
                });

                setStolovi(s);
                setYScroll(window.pageYOffset);
            }
        }
    }

    const fetchData = async () => {
        try {
            const response = await fetch('api/sto/allStoloviAdmin'); // Replace with your API endpoint
            if (!response.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await response.json();

            if (data !== null) {

                setResponse(data);

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
                sto.left = left + (width * (sto.x / 100));
                sto.top = top + (width * (sto.y / 100)) + 60 - yScroll;

                sto.w = width * (sto.width / 100);
                sto.h = width * (sto.height / 100);

                s.push(sto)
            });

            setCurrentDeoSaleId(id);
            setCurrentDeoSaleName(deoSale.name);
            setStolovi(s);
        }
    }

    const handleSubmitSave = async () => {
        try {
            setLoading(true);

            const s = [];

            stolovi.map(sto => {

                const x = (sto.left - left) * 100 / width;
                const y = (sto.top - top - 60 + yScroll) * 100 / width; 
                const h = sto.h * 100 / width;
                const w = sto.w * 100 / width;

                console.log(y)

                const ss = {
                    id: sto.id,
                    y: y,
                    x: x,
                    height: h,
                    width: w,
                    name: sto.name,
                    deoSaleId : sto.deoSaleId
                }

                s.push(ss);
            })

            const requestOptions = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(s)
            };

            const response = await fetch('api/sto/update', requestOptions); // Replace with your API endpoint
            if (!response.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }
            setDeloviSale([]);
            setStolovi([]);

            const response1 = await fetch('api/sto/allStolovi'); // Replace with your API endpoint
            if (!response1.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await response1.json();

            if (data !== null) {

                const n = []

                data.map((deoSale) => {
                    n.push(deoSale);
                })

                setDeloviSale(n);
            }

            const ss = []

            var deoSale = data.find(g => g.id === currentDeoSaleId);

            if (deoSale !== null) {
                deoSale.stolovi.map((sto) => {
                    sto.left = left + (width * (sto.x / 100));
                    sto.top = top + (width * (sto.y / 100)) + 60;

                    sto.w = width * (sto.width / 100);
                    sto.h = width * (sto.height / 100);

                    ss.push(sto)
                })
            }

            setStolovi(ss)

            toast.success('Uspešno ste sačuvali', { autoClose: 1000 });

            setCurrentSto(null);
            setWidthSto(0);
            setHeightSto(0);

            setLoading(false);
        } catch (error) {
            setError("Desila se greška: " + error.message);
            setLoading(false);
        }
    }

    const changeSize = (sto, rect) => {

        setCurrentSto(sto);
        setWidthSto(rect.width);
        setHeightSto(rect.height);
    }

    const changePosition = (id, rect) => {

        const s = [];
        setStolovi([]);

        stolovi.map(sto => {
            if (sto.id === id) {
                sto.left = rect.x;
                sto.top = rect.y;
            }

            s.push(sto);
        })

        setStolovi(s);
    }

    const theme = createTheme({
        overrides: {
            MuiOutlinedInput: {
                root: {
                    "&:hover $notchedOutline": {
                        borderColor: "white"
                    },
                    "&$focused $notchedOutline": {
                        borderColor: "white"
                    }
                },
                notchedOutline: {
                    borderColor: "white"
                }
            }
        }
    });

    const handleTextFieldWidthChange = function (e) {

        setWidthSto(e.target.value)

        const s = [];
        setStolovi([]);

        stolovi.map(sto => {
            if (sto.id === currentSto.id) {
                sto.w = Number(e.target.value);
            }

            s.push(sto);
        })

        setStolovi(s);
    }

    const handleTextFieldHeightChange = function (e) {

        setHeightSto(e.target.value)

        const s = [];
        setStolovi([]);

        stolovi.map(sto => {
            if (sto.id === currentSto.id) {
                sto.h = Number(e.target.value);
            }

            s.push(sto);
        })

        setStolovi(s);
    }

    const handleClose = () => {
        setOpen(false);
    }

    const handleTextFileDeoSaleName = function (e) {

        const d = {
            name: e.target.value
        };

        setNewDeoSale(d);
    }

    const openAddDeoSale = () => {

        setOpen(true);
        const d = {
            name: ''
        };

        setNewDeoSale(d);

    }

    const onClickAddDeoSale = async () => {

        setStolovi([]);
        setDeloviSale([]);

        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newDeoSale)
        };

        const response = await fetch('api/sto/addDeoSale', requestOptions); // Replace with your API endpoint
        if (!response.ok) {

            toast.error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži', { autoClose: 2000 });
            throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
        }

        const response1 = await fetch('api/sto/allStolovi'); // Replace with your API endpoint
        if (!response1.ok) {
            throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
        }

        const data = await response1.json();

        if (data !== null) {

            const n = []

            data.map((deoSale) => {
                n.push(deoSale);
            })

            setDeloviSale(n);
        }

        const s = []

        var deoSale = data.find(g => g.id === currentDeoSaleId);

        if (deoSale !== null) {
            deoSale.stolovi.map((sto) => {
                sto.left = left + (width * (sto.x / 100));
                sto.top = top + (width * (sto.y / 100)) + 60;

                sto.w = width * (sto.width / 100);
                sto.h = width * (sto.height / 100);

                s.push(sto)
            })
        }

        setStolovi(s)

        setOpen(false);

        toast.success('Uspešno dodat deo sale', { autoClose: 1000 });
    }

    const onClickAddSto = async () => {

        if (currentDeoSaleId > 0) {
            const newSto = {
                deoSaleId: currentDeoSaleId
            }

            const requestOptions = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newSto)
            };

            const response = await fetch('api/sto/addSto', requestOptions); // Replace with your API endpoint
            if (!response.ok) {

                toast.error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži', { autoClose: 2000 });
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const response1 = await fetch('api/sto/allStolovi'); // Replace with your API endpoint
            if (!response1.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await response1.json();

            if (data !== null) {

                const n = []

                data.map((deoSale) => {
                    n.push(deoSale);
                })

                setDeloviSale(n);
            }

            const s = []

            var deoSale = data.find(g => g.id === currentDeoSaleId);

            if (deoSale !== null) {
                deoSale.stolovi.map((sto) => {
                    sto.left = left + (width * (sto.x / 100));
                    sto.top = top + (width * (sto.y / 100)) + 60;

                    sto.w = width * (sto.width / 100);
                    sto.h = width * (sto.height / 100);

                    s.push(sto)
                })
            }

            setStolovi(s)

            toast.success('Uspešno dodat sto', { autoClose: 1000 });
        }
        else {
            toast.error('Izaberite deo sale', { autoClose: 2000 });
        }
    }

    return (
        <div className="App">
            <div className="appForm2">
                <div className="centerB">
                    {/*<Button variant="outlined" onClick={openAddDeoSale}>*/}
                    {/*    Dodaj deo sale*/}
                    {/*</Button>*/}
                </div>

                <Dialog
                    open={open}
                    onClose={handleClose}
                    PaperProps={{
                        component: 'form',
                        onSubmit: (event) => {
                            event.preventDefault();
                            const formData = new FormData(event.currentTarget);
                            const formJson = Object.fromEntries(formData.entries());
                            const email = formJson.email;
                            console.log(email);
                            handleClose();
                        },
                    }}
                >
                    <DialogTitle>Dodavanje dela sale</DialogTitle>
                    <DialogContent>
                        <DialogContentText>
                            Unesite naziv novog dela sale
                        </DialogContentText>
                        <TextField
                            autoFocus
                            required
                            margin="dense"
                            id="name"
                            name="nameDeoSale"
                            label="Naziv dela sale"
                            value={newDeoSale.name}
                            onChange={(e) => handleTextFileDeoSaleName(e)}
                            fullWidth
                            variant="standard"
                        />
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleClose}>Zatvori</Button>
                        {/*<Button type="submit" onClick={onClickAddDeoSale}>Dodaj deo sale</Button>*/}
                    </DialogActions>
                </Dialog>
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

                <div className="centerB"
                    style={{
                        margin: 10
                    }}>
                    {currentDeoSaleId === null ? <p className="formTitleLink"> Deo sale nije izabran </p> :
                        <p className="formTitleLink"> {currentDeoSaleName} </p>}
                </div>

                <div ref={ref} id="sala" className="pozicijaStolova" style={{
                    height: width
                }}>
                    {stolovi.map((sto) => (
                        <Draggable key={sto.id}
                            onDragStart={(rect) => {

                                changeSize(sto, rect);
                            }
                            }
                            onDragEnd={(rect) => {
                                changePosition(sto.id, rect)
                            }
                            }
                            style={{
                                left: sto.left,
                                top: sto.top
                            }}
                        >
                            <div style={{
                                backgroundColor: '#EC8B5E',
                                height: sto.h,
                                width: sto.w
                            }}>{sto.name}</div>
                        </Draggable>
                    ))}
                </div>

                <div className="centerB"
                    style={{
                        margin: 10
                    }}                >
                    {/*<Button variant="outlined" onClick={onClickAddSto}>*/}
                    {/*    Dodaj sto*/}
                    {/*</Button>*/}
                </div>
                <div className="centerB"
                    style={{
                        margin: 10
                    }}>
                    {currentSto === null ? <p className="formTitleLink"> Sto nije izabran </p> :
                        <p className="formTitleLink"> Sto {currentSto.name} </p>}
                </div>

                <div className="centerB"
                    style={{
                        margin: 10
                    }}>
                    <TextField
                        type="number"
                        sx={{
                            input: {
                                color: 'white'
                            }
                        }}
                        value={widthSto}
                        onChange={handleTextFieldWidthChange}
                        InputLabelProps={{
                            shrink: true,
                            sx: {
                                color: "white"
                            },
                            style: { color: "white" }
                        }}
                        label="Širina"
                        id="outlined-size-small"
                        size="small"
                    />
                </div>
                <div className="centerB"
                    style={{
                        margin: 10
                    }}>
                    <TextField theme={theme}
                        type="number"
                        sx={{
                            input: {
                                color: 'white'
                            }
                        }}
                        value={heightSto}
                        onChange={handleTextFieldHeightChange}
                        InputLabelProps={{
                            sx: {
                                color: "white"
                            },
                            style: {
                                color: "white"
                            }
                        }}
                        label="Dužina"
                        id="outlined-size-small"
                        size="small"
                    />
                </div>

                <div className="centerB">
                    <button className="formFieldButtonGrupa"
                        onClick={() => handleSubmitSave()}>Sačuvaj</button>
                </div>
            </div>

            <ToastContainer />
        </div>
    );
}
export default Admin;