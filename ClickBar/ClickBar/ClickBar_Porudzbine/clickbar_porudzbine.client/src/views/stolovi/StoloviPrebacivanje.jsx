import { useState, useEffect, useRef } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import TextField from '@mui/material/TextField';
import { styled, createTheme, ThemeProvider } from '@mui/material/styles';
import List from '@mui/material/List';
import './Stolovi.css'
import { ScrollMenu } from 'react-horizontal-scrolling-menu';
import 'react-horizontal-scrolling-menu/dist/styles.css';
import { DraggableFix } from '../../components/DraggableFix';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const theme = createTheme({
    components: {
        MuiOutlinedInput: {
            styleOverrides: {
                root: {
                    '&:hover .MuiOutlinedInput-notchedOutline': {
                        borderColor: 'white'
                    },
                    '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: 'white'
                    }
                },
                notchedOutline: {
                    borderColor: 'white'
                }
            }
        }
    }
});

const StoloviPrebacivanje = () => {
    const navigate = useNavigate()
    let location = useLocation();
    const ref = useRef(null);
    const [left, setLeft] = useState(0);
    const [top, setTop] = useState(0);
    const [width, setWidth] = useState(0);


    const [porudzbinaOriginal, setPorudzbinaOriginal] = useState(location.state.porudzbina);
    const [porudzbina, setPorudzbina] = useState(location.state.porudzbina);

    const [saStola, setSaStola] = useState(null);
    const [naSto, setNaSto] = useState(null);
    const [isSaStola, setIsSaStola] = useState(true);

    const [deloviSale, setDeloviSale] = useState([]);
    const [currentDeoSaleId, setCurrentDeoSaleId] = useState(null);
    const [stolovi, setStolovi] = useState([]);

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
            }
        }
    }

    const fetchData = async () => {
        try {
            setLoading(true);
            const response = await fetch('api/sto/allStolovi'); // Replace with your API endpoint
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
                sto.left = left + (width * (sto.x / 100));
                sto.top = top + (width * (sto.y / 100)) + 60;

                sto.w = width * (sto.width / 100);
                sto.h = width * (sto.height / 100);

                s.push(sto)
            })
        }
        setCurrentDeoSaleId(id);
        setStolovi(s)
    }

    const handleSubmitSto = (sto) => {
        console.log("STO:");
        console.log(sto);

        if (isSaStola) {
            setSaStola(sto);
        }
        else {
            setNaSto(sto);
        }

        setIsSaStola(!isSaStola);
    }

    const handleSubmitPrebaci = async () => {

        if (saStola !== null &&
            naSto !== null) {

            try {
                const movePorudzbine = {
                    fromSto: saStola,
                    toSto: naSto
                }
                const requestOptions1 = {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(movePorudzbine)
                };

                const response1 = await fetch('api/sto/moveOrder', requestOptions1); // Replace with your API endpoint
                if (!response1.ok) {

                    toast.error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži', { autoClose: 2000 });
                    throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
                }

                toast.success('Uspešno ste prebacili porudzbine na drugi sto!', { autoClose: 1000 });
                await timeout(1500);

                navigate('/sto', { state: { porudzbina: porudzbinaOriginal } });
            }
            catch (error) {
                console.log(error)
                toast.error('Desila se greška, proverite konekciju sa internetom.', { autoClose: 2000 });
            }
        }
        else {
            toast.error('Niste označili stolove.', { autoClose: 2000 });
        }
    }

    function timeout(delay) {
        return new Promise(res => setTimeout(res, delay));
    }

    const clickOnBack = () => {

        navigate('/sto', { state: { porudzbina: porudzbinaOriginal } });
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
        <ThemeProvider theme={theme}>
            <div className="App213">
                <div className="appForm123">
                    {/* ... ostatak komponente */}
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

                    <div ref={ref} id="sala" className="pozicijaStolova" style={{ height: width }}>
                        {stolovi.map((sto) => (
                            <DraggableFix key={sto.id}
                                style={{
                                    left: sto.left,
                                    top: sto.top
                                }}
                            >
                                <div onClick={() => handleSubmitSto(sto)}
                                    style={{
                                        backgroundColor: sto.color,
                                        height: sto.h,
                                        width: sto.w
                                    }}>{sto.name}</div>
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
                                <button key={sto.id}
                                    className="formFieldButtonNadgrupa"
                                    style={{ backgroundColor: sto.color }}
                                    onClick={() => handleSubmitSto(sto)}>{sto.name}</button>
                            ))}
                        </List>
                    </div>

                    <div className="centerB" style={{ margin: 10 }}>
                        <p className="formTitleLink"> Prebacivanje na sto </p>
                    </div>

                    <div className="centerB" style={{ margin: 10 }}>
                        <TextField
                            sx={{ input: { color: 'white' } }}
                            value={saStola === null ? "Selektuj sto" : saStola.name}
                            InputLabelProps={{
                                shrink: true,
                                sx: { color: "white" },
                                style: { color: "white" }
                            }}
                            label="Sa stola"
                            id="outlined-size-small"
                            size="small"
                        />
                    </div>

                    <div className="centerB" style={{ margin: 10 }}>
                        <TextField
                            sx={{ input: { color: 'white' } }}
                            value={naSto === null ? "Selektuj sto" : naSto.name}
                            InputLabelProps={{
                                sx: { color: "white" },
                                style: { color: "white" }
                            }}
                            label="Na sto"
                            id="outlined-size-small"
                            size="small"
                        />
                    </div>

                    <div className="centerB">
                        <button className="formFieldButtonGrupa" onClick={handleSubmitPrebaci}>Prebaci</button>
                    </div>
                </div>

                <ToastContainer />
            </div>
        </ThemeProvider>
    );
}
export default StoloviPrebacivanje;