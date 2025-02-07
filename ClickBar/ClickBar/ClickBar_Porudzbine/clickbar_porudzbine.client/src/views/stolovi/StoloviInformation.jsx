import { useState, useEffect, useRef } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import Box from '@mui/material/Box';
import { styled, ThemeProvider, createTheme } from '@mui/material/styles';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Paper from '@mui/material/Paper';
import './Stolovi.css'
import { ScrollMenu } from 'react-horizontal-scrolling-menu';
import 'react-horizontal-scrolling-menu/dist/styles.css';
import KeyboardArrowDown from '@mui/icons-material/KeyboardArrowDown';
import Button from '@mui/material/Button';
import { DraggableFix } from '../../components/DraggableFix';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const StoloviInformation = () => {
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

    const handleSubmitSto = (sto) => {
        console.log("STO:");
        console.log(sto);

        setOpen(true);
        setCurrentSto(sto);

        //navigate('/artikli', { state: { porudzbina: porudzbina } });
    }

    const clickOnBack = () => {

        navigate('/sto', { state: { porudzbina: porudzbinaOriginal } });
    }

    const openCurrentStoZelje = (index) => {

        const art = [];

        currentSto.items.map((a, i) => {
            if (i === index) {
                a.isOpen = !a.isOpen;
            }
            art.push(a);
        })

        setPorudzbina((p) => ({
            ...p,
            "artikli": art
        }));
    }

    const onClickNaplati = async () => {

        try {
            const sto = {
                name: currentSto.sto.name,
                id: currentSto.sto.id
            }

            const requestOptions1 = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(sto)
            };

            const response1 = await fetch('api/sto/pay', requestOptions1); // Replace with your API endpoint
            if (!response1.ok) {

                toast.error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži', { autoClose: 2000 });
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            toast.success('Uspešno ste naplatili!', { autoClose: 1000 });
            await timeout(1500);

            navigate('/sto', { state: { porudzbina: porudzbinaOriginal } });
        }
        catch (error) {
            console.log(error)
            toast.error('Desila se greška, proverite konekciju sa internetom.', { autoClose: 2000 });
        }
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
                    <DialogTitle>Porudzbine sa stola {currentSto === null ? null : currentSto.sto.name}</DialogTitle>
                    <DialogContent>
                        <div className="dugmici">
                            <List
                                sx={{ width: '100%' }}
                                component="nav"
                                aria-labelledby="nested-list-subheader"
                            >
                                {currentSto === null ? null : 
                                    currentSto.items.map((artikal, index) => (
                                        <ListItem
                                            key={index}
                                            disableGutters>
                                        <Box sx={{
                                            display: 'flex',
                                            mb: '5px',
                                            width: '100%'
                                        }}>
                                            <ThemeProvider
                                                theme={createTheme({
                                                    components: {
                                                        MuiListItemButton: {
                                                            defaultProps: {
                                                                disableTouchRipple: true,
                                                            },
                                                        },
                                                    },
                                                    palette: {
                                                        mode: 'dark',
                                                        primary: { main: 'rgb(102, 157, 246)' },
                                                        background: { paper: 'rgb(5, 30, 52)' }
                                                    },
                                                })}
                                            >
                                                <Paper elevation={0} sx={{ width: '100%' }}>
                                                    <FireNav component="nav" disablePadding>
                                                        <Divider />
                                                        <Box
                                                            sx={{
                                                                bgcolor: artikal.isOpen ? 'rgba(71, 98, 130, 0.2)' : null,
                                                                pb: artikal.isOpen ? 2 : 0,
                                                            }}
                                                        >
                                                            <ListItemButton
                                                                alignItems="flex-start"
                                                                onClick={() => openCurrentStoZelje(index)}
                                                                sx={{
                                                                    px: 3,
                                                                    pt: 2.5,
                                                                    pb: artikal.isOpen ? 0 : 2.5,
                                                                    '&:hover, &:focus': { '& svg': { opacity: artikal.isOpen ? 1 : 0 } },
                                                                }}
                                                            >
                                                                <ListItemText
                                                                    primary={artikal.nameFront}
                                                                    primaryTypographyProps={{
                                                                        fontSize: 15,
                                                                        fontWeight: 'medium',
                                                                        lineHeight: '20px',
                                                                        mb: '2px',
                                                                    }}
                                                                    secondaryTypographyProps={{
                                                                        noWrap: true,
                                                                        fontSize: 12,
                                                                        lineHeight: '16px',
                                                                        color: artikal.isOpen ? 'rgba(0,0,0,0)' : 'rgba(255,255,255,0.5)',
                                                                    }}
                                                                    sx={{ my: 0 }}
                                                                />
                                                                <KeyboardArrowDown
                                                                    sx={{
                                                                        mr: -1,
                                                                        opacity: 0,
                                                                        transform: artikal.isOpen ? 'rotate(-180deg)' : 'rotate(0)',
                                                                        transition: '0.2s',
                                                                    }}
                                                                />
                                                            </ListItemButton>
                                                            {artikal.isOpen &&
                                                                    artikal.zeljeFront.map((zelja, index) => (
                                                                    <ListItemButton
                                                                            key={index}
                                                                        sx={{ py: 0, minHeight: 32, color: 'rgba(255,255,255,.8)' }}
                                                                    >
                                                                        <ListItemIcon />
                                                                        <ListItemText
                                                                            primary={zelja}
                                                                            primaryTypographyProps={{ fontSize: 14, fontWeight: 'medium' }}
                                                                        />
                                                                    </ListItemButton>
                                                                ))}
                                                        </Box>
                                                    </FireNav>
                                                </Paper>
                                            </ThemeProvider>
                                        </Box>
                                    </ListItem>
                                ))}
                            </List>
                        </div>
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleClose}>Zatvori</Button>
                        {/*<Button type="submit" onClick={onClickNaplati}>Naplati</Button>*/}
                    </DialogActions>
                </Dialog>

                <div className="centerB">
                    <p className="formTitleLink">Pregled stolova</p>
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
export default StoloviInformation;