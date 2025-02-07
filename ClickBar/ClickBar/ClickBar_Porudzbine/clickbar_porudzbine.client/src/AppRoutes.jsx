import { Home } from "./components/Home";
import Artikli from "./views/artikli/Artikli"
import Sto from "./views/stolovi/Stolovi"
import Admin from "./views/admin/Admin"
import StoInformation from "./views/stolovi/StoloviInformation"
import StoPrebacivanje from "./views/stolovi/StoloviPrebacivanje"
import StoloviPrebacivanjeRadnika from "./views/stolovi/StoloviPrebacivanjeRadnika"
import DownloadLink from "./components/DownloadLink";

const AppRoutes = [
  {
        index: true,
        element: <Home />
    },
    {
        path: '/sto',
        element: <Sto />
    },
    {
        path: '/stoInformation',
        element: <StoInformation />
    },
    {
        path: '/stoPrebacivanje',
        element: <StoPrebacivanje />
    },
    {
        path: '/stoPrebacivanjeRadnika',
        element: <StoloviPrebacivanjeRadnika />
    },
    {
        path: '/artikli',
        element: <Artikli />
    },
    {
        path: '/admin',
        element: <Admin />
    },
    {
        path: '/download',
        element: <DownloadLink />
    },
  //{
  //  path: '/fetch-data',
  //  element: <FetchData />
  //}
];

export default AppRoutes;
