import { useState, useEffect, useRef, useCallback } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import Box from '@mui/material/Box';
import { styled, ThemeProvider, createTheme } from '@mui/material/styles';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import Paper from '@mui/material/Paper';
import IconButton from '@mui/material/IconButton';
import './Stolovi.css'
import { ScrollMenu } from 'react-horizontal-scrolling-menu';
import 'react-horizontal-scrolling-menu/dist/styles.css';
import KeyboardArrowDown from '@mui/icons-material/KeyboardArrowDown';
import Button from '@mui/material/Button';
import { DraggableFix } from '../../components/DraggableFix';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import TableRestaurantIcon from '@mui/icons-material/TableRestaurant';
import PublishedWithChangesIcon from '@mui/icons-material/PublishedWithChanges';
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone';
import Badge from '@mui/material/Badge';
import music from '../../sound/notification.mp3'
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import VisibilityIcon from '@mui/icons-material/Visibility';
import PeopleOutlineIcon from '@mui/icons-material/PeopleOutline';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const Stolovi = () => {
    const navigate = useNavigate()
    let location = useLocation();
    const ref = useRef(null);
    const [left, setLeft] = useState(0);
    const [top, setTop] = useState(0);
    const [width, setWidth] = useState(0);

    const [notifications, setNotifications] = useState([]);

    const [porudzbina, setPorudzbina] = useState(location.state.porudzbina);
    const [response, setResponse] = useState([]);
    const [deloviSale, setDeloviSale] = useState([]);
    const [currentDeoSaleId, setCurrentDeoSaleId] = useState(null);
    const [stolovi, setStolovi] = useState([]);

    const [open, setOpen] = useState(false);

    const [openChangeUser, setOpenChangeUser] = useState(false);
    const [users, setUsers] = useState([
        {
            'Id': '-1',
            'Name': 'Izaberite konobara'
        }])
    const [userForChange, setUserForChange] = useState('');

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const checkIsFinish = async () => {
        try {

            const user = {
                id: porudzbina.user,
                name: porudzbina.userName,
            }

            const requestOptions = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(user)
            };

            const responseF = await fetch('api/porudzbina/checkIsFinish', requestOptions); // Replace with your API endpoint
            if (!responseF.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await responseF.json();

            if (data !== null) {

                const notifi = []

                data.map((por) => {

                    const not = notifications.find((n) => n.brPorudzbine === por.brPorudzbine);

                    if (not === undefined) {
                        const message = "Zavrsena je porudzbina br: " + por.brPorudzbine + " za sto: " + por.stoBr;

                        // enable vibration support
                        navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;

                        if (navigator.vibrate) {
                            // vibration API supported
                            navigator.vibrate(3000);
                        }

                        new Audio(music).play();
                        toast.info(message, { autoClose: 1500 });

                        notifi.push(por);
                    }
                    else {

                        notifi.push(por);
                    }

                })
                setNotifications(notifi);
            }

            setLoading(false);
        } catch (error) {
            setError("Desila se greška: " + error.message);
            setLoading(false);
        }
    };

    const handleSubmitDeoSale = useCallback((id) => {

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
    }, [deloviSale, left, width, top]);

    const fetchData = useCallback(async () => {
        try {
            //console.log("ucitavanje stolova");
            setLoading(true);
            const response = await fetch('api/sto/allStolovi'); // Replace with your API endpoint
            if (!response.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await response.json();

            if (data !== null) {
                //console.log("Data fetched:", data); // Log fetched data
                setResponse(data);

                const n = []

                data.map((deoSale) => {
                    n.push(deoSale);
                })

                setDeloviSale(n);

                if (currentDeoSaleId !== null) {
                    handleSubmitDeoSale(currentDeoSaleId);
                }
            }

            setLoading(false);
        } catch (error) {
            setError("Desila se greška: " + error.message);
            setLoading(false);
        }
    }, [currentDeoSaleId]);

    useEffect(() => {
        //NotificationContainer.clear();
        //fetchData();
        const { offsetWidth, offsetLeft, offsetTop } = ref.current;
        setWidth(offsetWidth);
        setLeft(offsetLeft);
        setTop(offsetTop);
    }, []);

    useEffect(() => {
        const interval = setInterval(() => {
            fetchData();
        }, 3000);

        return () => {
            clearInterval(interval);
        };

    }, [fetchData]);

    // Add useEffect to watch changes in deloviSale and currentDeoSaleId
    useEffect(() => {
        if (currentDeoSaleId !== null) {
            handleSubmitDeoSale(currentDeoSaleId);
        }
    }, [deloviSale, currentDeoSaleId, handleSubmitDeoSale]);

    useEffect(() => {
        const interval = setInterval(() => {
            checkIsFinish();
        }, 3000);

        return () => {
            clearInterval(interval);
        };

    }, [notifications]);

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

    const handleSubmitSto = (sto) => {
        console.log("STO:");
        console.log(sto);

        porudzbina.sto = sto.id;
        porudzbina.stoName = sto.name;

        navigate('/artikli', { state: { porudzbina: porudzbina } });
    }

    const clickOnSeenOrder = async (order) => {

        console.log(order)

        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(order)
        };

        try {
            const response = await fetch('api/porudzbina/seenOrder', requestOptions); // Replace with your API endpoint

            if (!response.ok) {

                toast.error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži', { autoClose: 2000 });
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            fetchData();
            setStolovi([]);

            setCurrentDeoSaleId(null);

            toast.success('Predata porudzbina!', { autoClose: 1000 });
        }
        catch (error) {
            console.log(error)
            toast.error('Desila se greška, proverite konekciju sa internetom.', { autoClose: 2000 });
        }
    }

    const clickOnNotifications = () => {
        setOpen(true);
    }

    const handleClose = () => {
        setOpen(false);
    }
    const handleCloseChangeUser = () => {
        setOpenChangeUser(false);
    }

    const clickOnBack = () => {

        navigate('/');
    }
    const clickOnOpenChangeUser = () => {

        navigate('/stoPrebacivanjeRadnika', { state: { porudzbina: porudzbina } });
        
    }

    function timeout(delay) {
        return new Promise(res => setTimeout(res, delay));
    }

    const clickOnChangeUser = async () => {
        if (userForChange !== null &&
            userForChange !== "") {

            try {
                const moveKonobar = {
                    fromKonobarId: porudzbina.user,
                    toKonobarId: userForChange,
                }

                console.log(userForChange)
                console.log(moveKonobar)

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

                toast.success('Uspešno ste prebacili porudzbine na drugog konobara!', { autoClose: 1000 });
                await timeout(1500);

                navigate('/');
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

    const clickOnTableInformation = () => {

        navigate('/stoInformation', { state: { porudzbina: porudzbina } });
    }

    const clickOnTablePrebacivanje = () => {

        navigate('/stoPrebacivanje', { state: { porudzbina: porudzbina } });
    }

    const StyledBadge = styled(Badge)(({ theme }) => ({
        '& .MuiBadge-badge': {
            right: -3,
            top: 13,
            border: `2px solid ${theme.palette.background.paper}`,
            padding: '0 4px',
        },
    }));

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
                    <DialogTitle>Notifikacije</DialogTitle>
                    <DialogContent>
                        <div className="dugmici">
                            <List
                                sx={{ width: '100%' }}
                                component="nav"
                                aria-labelledby="nested-list-subheader"
                            >
                                {notifications === null ? null :
                                    notifications.map((notification, index) => (
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
                                                                    bgcolor: notification.isOpen ? 'rgba(71, 98, 130, 0.2)' : null,
                                                                    pb: notification.isOpen ? 2 : 0,
                                                                }}
                                                            >
                                                                <ListItemButton
                                                                    alignItems="flex-start"
                                                                    sx={{
                                                                        px: 3,
                                                                        pt: 2.5,
                                                                        pb: 2.5,
                                                                        '&:hover, &:focus': { '& svg': { opacity: 0 } },
                                                                    }}
                                                                >
                                                                    <ListItemText
                                                                        primary={"Porudzbina broj " + notification.brPorudzbine + " za sto: " + notification.stoBr}
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
                                                                            color: 'rgba(255,255,255,0.5)',
                                                                        }}
                                                                        sx={{ my: 0 }}
                                                                    />
                                                                    <KeyboardArrowDown
                                                                        sx={{
                                                                            mr: -1,
                                                                            opacity: 0,
                                                                            transform: 'rotate(0)',
                                                                            transition: '0.2s',
                                                                        }}
                                                                    />
                                                                </ListItemButton>
                                                            </Box>
                                                        </FireNav>
                                                    </Paper>
                                                </ThemeProvider>
                                            </Box>
                                            < VisibilityIcon aria-label="comment" onClick={() => clickOnSeenOrder(notification)} />
                                        </ListItem>
                                    ))}
                            </List>
                        </div>
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleClose}>Zatvori</Button>
                    </DialogActions>
                </Dialog>

                <IconButton style={{ float: 'right' }}
                    onClick={() => clickOnNotifications() }>
                    <StyledBadge badgeContent={notifications.length} color="secondary">
                        <NotificationsNoneIcon
                            className="fa-plus-circle"
                            sx={{ fontSize: 40, color: 'white' }}/>
                    </StyledBadge>
                </IconButton>
                <div className="horizontalRow"  >
                    <ArrowBackIcon
                        onClick={clickOnBack}
                        className="fa-plus-circle"
                        sx={{ fontSize: 40 }}
                        style={{ float: 'left' }} />
                    <TableRestaurantIcon
                        onClick={clickOnTableInformation}
                        className="fa-plus-circle"
                        sx={{ fontSize: 40 }} />
                    {/*<PeopleOutlineIcon*/}
                    {/*    onClick={clickOnOpenChangeUser}*/}
                    {/*    className="fa-plus-circle"*/}
                    {/*    sx={{ fontSize: 40 }} />*/}
                    <PublishedWithChangesIcon
                        onClick={clickOnTablePrebacivanje}
                        className="fa-plus-circle"
                        sx={{ fontSize: 40 }}
                        style={{ float: 'right' }}/>
                </div>
                <div className="konobarDiv">
                    <p className="konobarP konobarDiv">Konobar: {porudzbina.userName}</p>
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
                            <DraggableFix key={sto.id}
                                style={{
                                    left: sto.left,
                                    top: sto.top
                                }}
                            >
                                <div onClick={() => handleSubmitSto(sto)}
                                    style={{
                                    backgroundColor: sto.color,//'#EC8B5E',
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
                                style={{
                                    backgroundColor: sto.color
                                }}
                                onClick={() => handleSubmitSto(sto)}>{sto.name}</button>
                        ))}
                    </List>
                </div>
            </div>

            <ToastContainer />
        </div>
    );
}
export default Stolovi;