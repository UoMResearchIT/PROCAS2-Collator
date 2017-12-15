using System.Web;
using System.Web.Optimization;

namespace PROCAS2
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/unob").Include(
                        "~/Scripts/jquery.unobtrusive*"));


            bundles.Add(new ScriptBundle("~/bundles/jqueryconfirm").Include(
                        "~/Scripts/jquery-confirm.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/bootstrap-datepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                      "~/Scripts/DataTables/jquery.datatables.min.js",
                      "~/Scripts/DataTables/datatables.bootstrap.min.js",
                      "~/Scripts/DataTables/dataTables.fixedColumns.min.js"
                    ));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/jquery-confirm.min.css",
                      "~/Content/bootstrap-datepicker3.min.css",
                      "~/Content/SiteCSS/site.css"));
        }
    }
}
