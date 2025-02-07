import { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";
import { useNavigate } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import { styled, ThemeProvider, createTheme } from '@mui/material/styles';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Paper from '@mui/material/Paper';
import './Artikli.css'
import { ScrollMenu} from 'react-horizontal-scrolling-menu';
import 'react-horizontal-scrolling-menu/dist/styles.css';
import KeyboardArrowDown from '@mui/icons-material/KeyboardArrowDown';
import Checkbox from '@mui/material/Checkbox';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import Button from '@mui/material/Button';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import FormControlLabel from '@mui/material/FormControlLabel';

const MyInputZelja = (props) => {
    return <TextField
        {...props} 
        variant="outlined"
        sx={{
            input: {
                color: 'white'
            }
        }}
        InputLabelProps={{
            sx: {
                color: "white"
            },
            style: {
                color: "white"
            }
        }}
        label="Želja"
        id="outlined-size-small"
        size="small"
    />
};

const Artikli = () => {
    let location = useLocation();
    const navigate = useNavigate()

    const [kolicina, setKolicina] = useState(1);
    const [porudzbina, setPorudzbina] = useState(location.state.porudzbina);
    const [response, setResponse] = useState([]);
    const [nadgupe, setNadgupe] = useState([]);
    const [grupe, setGrupe] = useState([]);
    const [currentGrupaId, setCurrentGrupaId] = useState(null);
    const [artikli, setArtikli] = useState([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const [oneClick, setOneClick] = useState(false);

    useEffect(() => {
        fetchData();
    }, []);

    const fetchData = async () => {
        try {
            const response = await fetch('api/artikal/allArtikli'); // Replace with your API endpoint
            if (!response.ok) {
                throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
            }

            const data = await response.json();

            if (data !== null) {

                setResponse(data);

                const n = []

                data.map((nadgrup) =>
                {
                    n.push(nadgrup);
                })

                setNadgupe(n);
            }

            setLoading(false);
        } catch (error) {
            setError("Desila se greška: " + error.message);
            setLoading(false);
        }
    };

    const handleSubmitNadgrupa = (id) => {

        const g = []

        var nadgrupa = nadgupe.find(n => n.id === id);

        setArtikli([])

        if (nadgrupa !== null) {
            nadgrupa.grupe.map((grupa) => {
                g.push(grupa)
            })
        }
        setGrupe(g)
    }

    const handleSubmitGrupa = (id) => {

        const a = []

        var grupa = grupe.find(g => g.id === id);

        if (grupa !== null) {
            grupa.artikli.map((artikal) => {
                a.push(artikal)
            })
        }
        setCurrentGrupaId(id);
        setArtikli(a)
    }

    const openArtikal = (artikal) => {

        const art = [];

        artikli.map((a) => {
            if (a.id === artikal.id) {
                a.isOpen = !a.isOpen;
            }
            art.push(a);
        })

        setArtikli(art);
    }

    const openArtikalPorudzbine = (index) => {

        const art = [];

        porudzbina.artikli.map((a, i) => {
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
    
    const checkZelja = (zelja, artikal) => {

        const index = artikli.findIndex(a => a.id === artikal.id);

        const indexZelja = artikli[index].zelje.findIndex(z => z.id === zelja.id);
        artikli[index].zelje[indexZelja].isCheck = !artikli[index].zelje[indexZelja].isCheck;

        console.log(artikli[index].zelje[indexZelja].isCheck);

        setArtikli(artikli);
    }

    const handleAddArtikal = (artikal) => {

        const ar = []

        artikli.map(a => {
            const aaa =
            {
                brzoBiranje: a.brzoBiranje,
                id: a.id,
                id_String: a.id_String,
                isAdded: a.isAdded,
                isOpen: a.isOpen,
                jm: a.jm,
                mpc: a.mpc,
                name: a.name,
                totalOrderNumber: a.totalOrderNumber
            };
            if (a.id === artikal.id) {
                aaa.isAdded = true;
            }

            ar.push(aaa)
        })

        setArtikli(ar)

        const artikalPorudzbine = {
            'id': artikal.id,
            'id_String': artikal.id_String,
            'nameFront': artikal.name + " - " + kolicina + artikal.jm,
            'name': artikal.name,
            'jm': artikal.jm,
            'isOpen': false,
            'zelje': [],
            'mpc': artikal.mpc,
            'kolicina': Number(kolicina),
        }
        artikal.zelje.map((zelja) => {
            if (zelja.isCheck === true) {
                
                artikalPorudzbine.zelje.push({
                    'id': zelja.id,
                    'artikalId': zelja.artikalId,
                    'name': zelja.name,
                    'description': zelja.description
                })
                
            }
        })

        const artikliPorudzbine = [...porudzbina.artikli, artikalPorudzbine]

        setPorudzbina((p) => ({
            ...p,
            "artikli": artikliPorudzbine
        }));

        const a = []

        var grupa = grupe.find(g => g.id === currentGrupaId);

        if (grupa !== null) {
            grupa.artikli.map((artikal) => {
                artikal.zelje.map(z => {
                    z.isCheck = false;
                })
                artikal.isAdded = false;
                a.push(artikal)
            })
        }
        setArtikli(a);

        toast.success('Uspešno dodat artikal', { autoClose: 1000 });

        setKolicina(1);


        const listArt = [...artikli];

        listArt.map(ar => {
            ar.zelje.map(ze => {
                if (ze.id < 0) {
                    ze.description = ""
                }
            })
        })

        setArtikli(listArt);
    }


    const handleRemoveArtikal = (index) => {

        const art = porudzbina.artikli.filter((_, i) => i !== index)

        setPorudzbina((p) => ({
            ...p,
            "artikli": art
        }));
        toast.info('Obrisali ste artikal', { autoClose: 1000 });
    }

    const onClickZavrsi = async () => {

        if (oneClick === false) {
            setOneClick(true);

            console.log(oneClick);

            const poru = {
                radnikName: porudzbina.userName,
                radnikId: porudzbina.user,
                stoBr: porudzbina.stoName.toString(),
                items: []
            };

            porudzbina.artikli.map(artikalPor => {

                const a = {
                    kolicina: artikalPor.kolicina,
                    itemIdString: artikalPor.id_String,
                    jm: artikalPor.jm,
                    naziv: artikalPor.name,
                    mpc: artikalPor.mpc,
                    zelje: "",
                    rbs: 0,
                    brojNarudzbe: 0
                };

                let brojac = 0;

                artikalPor.zelje.map(z => {

                    if (brojac > 0) {
                        a.zelje += ", "
                    }
                    a.zelje += z.description;
                    brojac++;
                });

                poru.items.push(a);
            });

            const requestOptions = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(poru)
            };

            try {
                const response = await fetch('api/porudzbina/create', requestOptions); // Replace with your API endpoint

                if (!response.ok) {

                    toast.error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži', { autoClose: 2000 });
                    throw new Error('Greška prilikom komunikacije sa serverom. Proverite da li ste na istoj mreži.');
                }

                toast.success('Uspešno kreirana porudzbina', { autoClose: 1000 });

                await timeout(1500);

                const por = {
                    userName: porudzbina.userName,
                    user: porudzbina.user,
                    artikli: []
                }

                console.log(por)

                navigate('/sto', { state: { porudzbina: por } });
                //navigate('/');
            }
            catch (error) {
                console.log(error)
                toast.error('Desila se greška, proverite konekciju sa internetom.', { autoClose: 2000 });
            }

            setOneClick(false);
        }
    }

    function timeout(delay) {
        return new Promise(res => setTimeout(res, delay));
    }

    const handleTextFieldZeChange = function (artikal, zelja, value) {

        const listArt = [...artikli];

        const indexArt = listArt.findIndex(a => a.id === artikal.id);
        const indexZelj = listArt[indexArt].zelje.findIndex(z => z.id === zelja.id);

        listArt[indexArt].zelje[indexZelj].description = value;

        setArtikli(listArt);
    }

    const handleTextFieldKolicinaChange = function (e) {

        setKolicina(e.target.value)
    }

    const clickOnBack = () => {

        navigate('/sto', { state: { porudzbina: porudzbina } });
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
        <div className="App1">
            <div className="appForm1">
                <ArrowBackIcon
                    onClick={clickOnBack}
                    className="fa-plus-circle"
                    sx={{ fontSize: 40 }} />
                <div className="konobarDiv">
                    <p className="konobarP konobarDiv">Konobar: {porudzbina.userName}</p>
                    <p className="konobarP konobarDiv">Sto: {porudzbina.stoName}</p>
                </div>
                {porudzbina.artikli.length > 0 ?
                    <div className="dugmici">
                        <p className="formTitleLink centerB"> Poručeno: </p>
                        <List
                            sx={{ width: '100%' }}
                            component="nav"
                            aria-labelledby="nested-list-subheader"
                        >
                            {porudzbina.artikli.map((artikal, index) => (
                                <ListItem
                                    disableGutters
                                    key={index}>
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
                                                            onClick={() => openArtikalPorudzbine(index)}
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
                                                            artikal.zelje.map((zelja) => (
                                                                <ListItemButton
                                                                    key={zelja.id}
                                                                    sx={{ py: 0, minHeight: 32, color: 'rgba(255,255,255,.8)' }}
                                                                >
                                                                    <ListItemIcon />
                                                                    <ListItemText
                                                                        primary={zelja.description}
                                                                        primaryTypographyProps={{ fontSize: 14, fontWeight: 'medium' }}
                                                                    />
                                                                </ListItemButton>
                                                            ))}
                                                    </Box>
                                                </FireNav>
                                            </Paper>
                                        </ThemeProvider>
                                    </Box>
                                    < DeleteOutlineIcon aria-label="comment" onClick={() => handleRemoveArtikal(index)} />
                                </ListItem>
                            ))}
                        </List>
                        <Box textAlign='center'>
                            <Button onClick={() => onClickZavrsi()}
                                style={{
                                    backgroundColor: "#478B71"
                                }}
                                variant="contained" >
                                Završi
                            </Button>
                        </Box>
                    </div> : null
                }
                <div className="dugmici">
                    {nadgupe.map((nadgrupa) => (
                            <button key={nadgrupa.id}
                                className="formFieldButtonNadgrupa1"
                                onClick={() => handleSubmitNadgrupa(nadgrupa.id)}>{nadgrupa.name}</button>
                    ))}
                </div>

                <div className="dugmici">
                    <p className="formTitleLink"> Grupe: </p>
                    <ScrollMenu>
                        {grupe.map((grupa) => (
                            <div key={grupa.id}
                                className="rows">
                                <button className="formFieldButtonGrupa1"
                                        onClick={() => handleSubmitGrupa(grupa.id)}>{grupa.name}</button>
                            </div>
                        ))}
                    </ScrollMenu >
                </div>

                <div className="dugmici">
                    <p className="formTitleLink "> Artikli: </p>
                    <div className="centerB"
                        style={{
                            margin: 10
                        }}>
                        <TextField 
                            type="number"
                            variant="outlined"
                            sx={{
                                input: {
                                    color: 'white'
                                }
                            }}
                            value={kolicina}
                            onChange={handleTextFieldKolicinaChange}
                            InputLabelProps={{
                                sx: {
                                    color: "white"
                                },
                                style: {
                                    color: "white"
                                }
                            }}
                            label="Količina"
                            id="outlined-size-small"
                            size="small"
                        />
                    </div>
                        <List
                            sx={{ width: '100%' }}
                            component="nav"
                            aria-labelledby="nested-list-subheader"
                        >                            
                        {artikli.map((artikal) => (
                            <ListItem
                                disableGutters
                                key={artikal.id}>
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
                                                background: { paper: '#EC8B5E' }
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
                                                        onClick={() => openArtikal(artikal)}
                                                        sx={{
                                                            px: 3,
                                                            pt: 2.5,
                                                            pb: artikal.isOpen ? 0 : 2.5,
                                                            '&:hover, &:focus': { '& svg': { opacity: artikal.isOpen ? 1 : 0 } },
                                                        }}
                                                    >
                                                        <ListItemText
                                                            primary={artikal.name}
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
                                                        artikal.zelje.map((zelja, index) => (
                                                            <ListItemButton
                                                                dense
                                                                key={zelja.id}
                                                                sx={{ py: 0, minHeight: 32, color: 'rgba(255,255,255,.8)' }}
                                                            >
                                                                
                                                                {zelja.name !== "Dodaj zelju" ? <FormControlLabel
                                                                    style={{ width: "100%" }}
                                                                    label={zelja.name}
                                                                    control={<ListItemIcon>
                                                                        <Checkbox
                                                                            label={zelja.name}
                                                                            edge="start"
                                                                            defaultChecked={zelja.isCheck}
                                                                            onChange={() => checkZelja(zelja, artikal)}
                                                                        />
                                                                    </ListItemIcon>}
                                                                /> :
                                                                    <FormControlLabel
                                                                        style={{ width: "100%" }}
                                                                        label={<MyInputZelja
                                                                            key={index}
                                                                            defaultValue={zelja.description}
                                                                            onBlur={(e) => handleTextFieldZeChange(artikal, zelja, e.target.value)} />}
                                                                        control={<ListItemIcon>
                                                                            <Checkbox
                                                                                label={zelja.name}
                                                                                edge="start"
                                                                                defaultChecked={zelja.isCheck}
                                                                                onChange={() => checkZelja(zelja, artikal)}
                                                                            />
                                                                        </ListItemIcon>}
                                                                    />
                                                                }
                                                            </ListItemButton>
                                                        ))}
                                                </Box>
                                            </FireNav>
                                        </Paper>
                                    </ThemeProvider>
                                </Box>
                                <AddIcon aria-label="comment" onClick={() => handleAddArtikal(artikal)} />
                                
                            </ListItem>
                        ))}
                    </List>
                </div>
            </div>

            <ToastContainer />
        </div>
    );
}
export default Artikli;