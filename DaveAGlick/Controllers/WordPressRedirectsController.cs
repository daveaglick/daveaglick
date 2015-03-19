using AttributeRouting.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaveAGlick.Controllers
{
    public partial class WordPressRedirectsController : Controller
    {
        [GET("/2014/05/30/method-chaining-fluent-interfaces-and-the-finishing-problem")]
        public virtual ActionResult Wp20140530()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.method_chaining_fluent_interfaces_and_the_finishing_problem));
        }

        [GET("2014/05/21/brace-style-convention-or-why-i-prefer-my-braces-on-their-own-line")]
        public virtual ActionResult Wp20140521()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.brace_style_convention));
        }

        [GET("2014/05/09/using-asp-net-mvc-and-razor-to-generate-pdf-files")]
        public virtual ActionResult Wp20140509()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.using_aspnet_mvc_and_razor_to_generate_pdf_files));
        }

        [GET("2014/03/06/how-i-export-kendo-grids-to-excel-or-csv")]
        public virtual ActionResult Wp20140306()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.how_i_export_kendo_grids_to_excel_or_csv));
        }

        [GET("2014/01/03/how-to-post-data-including-checkboxes-in-a-kendoui-grid-back-to-an-asp-net-mvc-action")]
        public virtual ActionResult Wp20140103()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.how_to_post_data_in_a_kendoui_grid));
        }

        [GET("2014/01/02/strongly-typed-icon-fonts-in-asp-net-mvc-2")]
        public virtual ActionResult Wp20140102()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.strongly_typed_icon_fonts_in_aspnet_mvc));
        }

        [GET("2013/05/17/round-robin-row-selection-from-sql-server")]
        public virtual ActionResult Wp20130517()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.round_robin_row_selection_from_sql_server));
        }

        [GET("2013/05/16/quick-and-dirty-multiple-value-dictionary-using-extension-methods")]
        public virtual ActionResult Wp20130516()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.quick_and_dirty_multiple_value_dictionary_using_extension_methods));
        }

        [GET("2013/05/13/custom-entity-type-configurations-in-entity-framework-code-first-part-2")]
        public virtual ActionResult Wp20130513()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.custom_entity_type_configurations_in_entity_framework_code_first_part_2));
        }

        [GET("2013/04/17/custom-entity-type-configurations-in-entity-framework-code-first-part-1")]
        public virtual ActionResult Wp20130417()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.custom_entity_type_configurations_in_entity_framework_code_first_part_1));
        }

        [GET("2013/04/11/automatically-generating-column-titles-for-a-kendoui-mvc-grid")]
        public virtual ActionResult Wp20130411()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.automatically_generating_column_titles_for_a_kendoui_mvc_grid));
        }

        [GET("2012/10/17/getting-an-htmlhelper-for-an-alternate-model-type")]
        public virtual ActionResult Wp20121017()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.getting_an_htmlhelper_for_an_alternate_model_type));
        }

        [GET("2012/03/26/object-persistence-in-nxd")]
        public virtual ActionResult Wp20120326()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.object_persistence_in_nxdb));
        }

        [GET("2012/02/24/introducing-nxdb")]
        public virtual ActionResult Wp20120224()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.introducing_nxdb));
        }

        [GET("2012/02/17/introducing-nicethreads")]
        public virtual ActionResult Wp20120217()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.introducing_nicethreads));
        }

        [GET("2010/06/17/xquery-function-to-get-the-number-of-weekwork-days")]
        public virtual ActionResult Wp20100617()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.xquery_function_to_get_the_number_of_week_work_days));
        }

        [GET("2010/04/15/nested-grabs-in-gtksharp")]
        public virtual ActionResult Wp20100415()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.nested_grabs_in_gtksharp));
        }

        [GET("2010/04/12/right-click-context-menus-in-gtksharp")]
        public virtual ActionResult Wp20100412()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.right_click_context_menus_in_gtksharp));
        }

        [GET("2010/04/09/exporting-a-treeview-to-csv")]
        public virtual ActionResult Wp20100409()
        {
            return RedirectToActionPermanent(this.PostAction(x => x.exporting_a_gtksharp_treeview_to_csv));
        }
    }
}