/*****************************************************************************
* © 2015 OSS, All rights reserved, For internal use only
*
* FILE: UnauthorisedController.cs
* PROJECT: SmartLivingSolution.Web (SmartLiving Solution Web Portal, Presentation Part)
* MODULE: Controllers
*
* Description: This is Controller for all actions of Unauthorised Part of this site.
*
* Notes: 
*
* Subroutines Called:
*
* Returns:
*
* Globals:
*
* Designer(s):/ Design Document:
* Programmer(s): Ahsanul Haq Shohel
* Tested By: Date:
* Assumptions and Limitation:
* Compiler dependencies or special instructions:
*
* REVISION HISTORY
* Date: By: Description:
*
*****************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcHRMS.Controllers
{
    public class UnauthorisedController : Controller
    {
        //
        // GET: /Unauthorised/

        // GET: Unauthorised
        public ActionResult Index()
        {
            Session.Abandon();
            return View();
        }
    }
}
